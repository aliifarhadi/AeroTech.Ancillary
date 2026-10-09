using System.Net;
using System.Text;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Providers;
using AeroTech.Ancillary.Providers.FlightFlow.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Inventory;

public class V122FlightOccurrenceReferenceAcceptanceTests
{
    private const int Owner = 1;
    private const long Flight = 1550116065952923736;

    private const string RecordedFlight = """
        {
          "data": {
            "id": "1550116065952923736",
            "flightNumber": "1598",
            "originAirportId": 6,
            "originAirportTerminalId": 6,
            "destinationAirportId": 2,
            "destinationAirportTerminalId": 2,
            "operatingAirlineId": 1,
            "marketingAirlineId": 3,
            "departureDateTime": "2026-10-10T19:40:00+03:00",
            "arrivalDateTime": "2026-10-10T23:33:00+03:30",
            "duration": 203,
            "aircraftId": 1,
            "flightStatus": 2
          },
          "errors": null
        }
        """;

    private static HttpResponseMessage Json(HttpStatusCode status, string body)
        => new(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") };

    private static (FlightFlowFlightOccurrenceReference Reference, Recorder Calls) Connected(Func<HttpRequestMessage, HttpResponseMessage> reply)
    {
        var recorder = new Recorder(reply);

        return (new FlightFlowFlightOccurrenceReference(new HttpClient(recorder) { BaseAddress = new Uri("https://flightflow.test/service/") }), recorder);
    }

    [Fact]
    public async Task V122_FF01_a_flight_operated_by_the_owner_is_verified_through_the_flightflow_service_route()
    {
        var (reference, calls) = Connected(_ => Json(HttpStatusCode.OK, RecordedFlight));

        Assert.Equal(InventoryReferenceCheck.Verified, await reference.CheckAsync(Owner, Flight));
        Assert.Equal(new[] { "GET https://flightflow.test/service/v1/Flights/1550116065952923736" }, calls.Requests);
    }

    [Fact]
    public async Task V122_FF02_an_unknown_flight_or_a_flight_of_another_operating_airline_is_not_found()
    {
        var (missing, _) = Connected(_ => Json(HttpStatusCode.NotFound, """{"status":404,"title":"Not Found"}"""));
        var (foreign, _) = Connected(_ => Json(HttpStatusCode.OK, RecordedFlight));

        Assert.Equal(InventoryReferenceCheck.NotFound, await missing.CheckAsync(Owner, 990000001));
        Assert.Equal(InventoryReferenceCheck.NotFound, await foreign.CheckAsync(3, Flight));
        Assert.Equal(InventoryReferenceCheck.NotFound, await foreign.CheckAsync(2, Flight));
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError, "{}")]
    [InlineData(HttpStatusCode.BadGateway, "")]
    [InlineData(HttpStatusCode.Unauthorized, "")]
    [InlineData(HttpStatusCode.OK, "{}")]
    [InlineData(HttpStatusCode.OK, """{"data":null,"errors":null}""")]
    [InlineData(HttpStatusCode.OK, """{"data":{"id":"7","operatingAirlineId":1}}""")]
    [InlineData(HttpStatusCode.OK, "<html>gateway</html>")]
    public async Task V122_FF03_an_answer_that_does_not_prove_the_flight_keeps_the_source_unavailable(HttpStatusCode status, string body)
    {
        var (reference, _) = Connected(_ => Json(status, body));

        Assert.Equal(InventoryReferenceCheck.SourceUnavailable, await reference.CheckAsync(Owner, Flight));
    }

    [Fact]
    public async Task V122_FF04_a_transport_failure_a_timeout_or_a_missing_base_address_keeps_the_source_unavailable()
    {
        var (unreachable, _) = Connected(_ => throw new HttpRequestException("connection refused"));
        var (slow, _) = Connected(_ => throw new TaskCanceledException("timeout"));
        var unconfigured = new FlightFlowFlightOccurrenceReference(new HttpClient(new Recorder(_ => Json(HttpStatusCode.OK, RecordedFlight))));

        Assert.Equal(InventoryReferenceCheck.SourceUnavailable, await unreachable.CheckAsync(Owner, Flight));
        Assert.Equal(InventoryReferenceCheck.SourceUnavailable, await slow.CheckAsync(Owner, Flight));
        Assert.Equal(InventoryReferenceCheck.SourceUnavailable, await unconfigured.CheckAsync(Owner, Flight));

        using var cancelled = new CancellationTokenSource();

        await cancelled.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => slow.CheckAsync(Owner, Flight, cancelled.Token));
    }

    [Fact]
    public void V122_FF05_the_host_registers_the_flightflow_reference_for_flights_and_keeps_the_four_other_references_unconnected()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["FlightFlow:BaseUrl"] = "https://flightflow.test/service/" })
            .Build();

        using var provider = new ServiceCollection().AddProviders(configuration).BuildServiceProvider();
        using var scope = provider.CreateScope();

        Assert.IsType<FlightFlowFlightOccurrenceReference>(scope.ServiceProvider.GetRequiredService<IFlightOccurrenceReference>());
        Assert.Equal(
            new[]
            {
                "NotConnectedAirportFacilityReference", "NotConnectedCountingFamilyReference", "NotConnectedFlightFlowDelegationReference", "NotConnectedInventoryResourceReference"
            },
            new object[]
                {
                    scope.ServiceProvider.GetRequiredService<IAirportFacilityReference>(),
                    scope.ServiceProvider.GetRequiredService<ICountingFamilyReference>(),
                    scope.ServiceProvider.GetRequiredService<IFlightFlowDelegationReference>(),
                    scope.ServiceProvider.GetRequiredService<IInventoryResourceReference>()
                }
                .Select(reference => reference.GetType().Name));
    }

    private sealed class Recorder : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _reply;

        public Recorder(Func<HttpRequestMessage, HttpResponseMessage> reply) => _reply = reply;

        public List<string> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Requests.Add($"{request.Method} {request.RequestUri}");

            return Task.FromResult(_reply(request));
        }
    }
}
