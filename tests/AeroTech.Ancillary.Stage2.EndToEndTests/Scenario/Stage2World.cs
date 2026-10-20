using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using AeroTech.Ancillary.ServiceHost.CallerContext;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Ancillary.Stage2.EndToEndTests.Host;
using AeroTech.Ancillary.Stage2.EndToEndTests.Ordering;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace AeroTech.Ancillary.Stage2.EndToEndTests.Scenario;

public sealed class Stage2World : IAsyncDisposable
{
    private static readonly Uri AncillaryAddress = new("http://ancillary.e2e/");
    private static readonly Uri OrderingAddress = new("http://ordering.e2e/");

    private readonly string _testName;
    private readonly AncillaryTestHost _ancillary;
    private readonly OrderingSimulator _ordering;
    private readonly HttpClient _orderingClient;
    private readonly HashSet<string> _expectedEscapes = new(StringComparer.Ordinal);

    private Stage2World(string testName, Stage2Ports ports, Transcript transcript, AncillaryTestHost ancillary, OrderingSimulator ordering)
    {
        _testName = testName;
        Ports = ports;
        Transcript = transcript;
        _ancillary = ancillary;
        _ordering = ordering;
        _orderingClient = new HttpClient(Chain(transcript.Recorder("test->ordering"), ordering.CreateHandler())) { BaseAddress = OrderingAddress };
    }

    public Stage2Ports Ports { get; }

    public Transcript Transcript { get; }

    public ShoppingLab Lab => Ports.Lab;

    public static async Task<Stage2World> StartAsync(ShoppingLab lab, bool verifiedExitRows = false, [CallerMemberName] string testName = "")
    {
        var ports = new Stage2Ports { Lab = lab, VerifiedExitRows = verifiedExitRows };
        var transcript = new Transcript();
        var ancillary = new AncillaryTestHost(ports);
        var resolveClient = ancillary.CreateDefaultClient(AncillaryAddress, transcript.Recorder("ordering->ancillary"));
        var ordering = await OrderingSimulator.StartAsync(ports.Orders, ports.AirAvail, resolveClient, ports.Clock);

        ports.Airports.IataCodes[2] = "IKA";
        ports.Airports.IataCodes[6] = "IST";
        ports.Airports.TerminalNumbers[2] = "1";
        ports.Airports.TerminalNumbers[6] = "2";
        lab.Catalog.AirlineCodes[1] = "DA";
        lab.Catalog.AirlineCodes[3] = "IR";

        return new Stage2World(testName, ports, transcript, ancillary, ordering);
    }

    public HttpClient Surface(string identity, long? countryId = null)
    {
        var client = _ancillary.CreateDefaultClient(AncillaryAddress, Transcript.Recorder($"{identity}->ancillary"));

        client.DefaultRequestHeaders.Add(E2EIdentities.Header, identity);

        if (countryId is { } country)
            client.DefaultRequestHeaders.Add(HeaderRequestCountry.CountryHeader, country.ToString(System.Globalization.CultureInfo.InvariantCulture));

        return client;
    }

    public HttpClient Anonymous() => _ancillary.CreateDefaultClient(AncillaryAddress, Transcript.Recorder("anonymous->ancillary"));

    public static async Task<E2EResponse> PostAsync(HttpClient client, string route, object body)
    {
        using var response = await client.PostAsJsonAsync(route, body);

        return await ReadAsync(response);
    }

    public Task<E2EResponse> OrderingPostAsync(string route, object body) => PostAsync(_orderingClient, route, body);

    public async Task<E2EResponse> OrderingGetAsync(string route)
    {
        using var response = await _orderingClient.GetAsync(route);

        return await ReadAsync(response);
    }

    public void ExpectEscape(string escape) => _expectedEscapes.Add(escape);

    public T HostService<T>()
        where T : notnull
    {
        using var scope = _ancillary.Services.CreateScope();

        return scope.ServiceProvider.GetRequiredService<T>();
    }

    public async ValueTask DisposeAsync()
    {
        var multiplexer = _ancillary.Services.GetService<IConnectionMultiplexer>();
        var escapes = Ports.Guard.Escapes.Where(escape => !_expectedEscapes.Contains(escape)).ToList();

        Transcript.Write(_testName, escapes.Count == 0 ? "isolated" : $"escaped: {string.Join(" | ", escapes)}");
        _orderingClient.Dispose();
        await _ordering.DisposeAsync();
        await _ancillary.DisposeAsync();

        if (multiplexer is not null)
            await multiplexer.DisposeAsync();

        if (escapes.Count > 0)
            throw new InvalidOperationException($"E2E isolation violated: {string.Join(" | ", escapes)}");
    }

    private static async Task<E2EResponse> ReadAsync(HttpResponseMessage response)
    {
        var text = await response.Content.ReadAsStringAsync();

        return new E2EResponse((int)response.StatusCode, string.IsNullOrWhiteSpace(text) ? null : JsonNode.Parse(text));
    }

    private static HttpMessageHandler Chain(DelegatingHandler outer, HttpMessageHandler inner)
    {
        outer.InnerHandler = inner;

        return outer;
    }
}
