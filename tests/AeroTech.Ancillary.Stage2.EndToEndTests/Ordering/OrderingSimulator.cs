using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Providers.Offer.Services;
using AeroTech.Ancillary.Stage2.EndToEndTests.Host;
using AeroTech.Framework.Core.Domain.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Logging;

namespace AeroTech.Ancillary.Stage2.EndToEndTests.Ordering;

public sealed class OrderingSimulator : IAsyncDisposable
{
    public const string Prefix = "/__e2e/orders";

    private readonly WebApplication _app;

    private OrderingSimulator(WebApplication app) => _app = app;

    public HttpMessageHandler CreateHandler() => _app.GetTestServer().CreateHandler();

    public static async Task<OrderingSimulator> StartAsync(TestOrderStore store, TestAirAvail airAvail, HttpClient ancillary, FixedClock clock)
    {
        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions { EnvironmentName = "E2E" });

        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();

        var app = builder.Build();
        var adds = new OrderingAddService(store, ancillary);
        var offers = new FlightOfferProvider(new HttpClient(new AirAvailHandler(airAvail)) { BaseAddress = Stage2Settings.AirAvailBaseAddress });

        app.MapPost($"{Prefix}/from-offer", async (CreateOrderFromOfferRequest request, CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.OfferId))
                return Results.UnprocessableEntity(new { error = "OfferIdRequired" });

            try
            {
                var order = store.Create(request.OfferId, await offers.GetByOfferIdAsync(request.OfferId, cancellationToken));

                return Results.Created($"{Prefix}/{order.OrderId}", TestOrderSnapshot.Of(order));
            }
            catch (BusinessException failure)
            {
                return Results.Json(new { error = "OfferNotUsable", detail = failure.Message }, statusCode: StatusCodes.Status422UnprocessableEntity);
            }
        });

        app.MapGet($"{Prefix}/{{orderId:long}}", (long orderId) =>
            store.Find(orderId) is { } order ? Results.Ok(TestOrderSnapshot.Of(order)) : Results.NotFound(new { error = "OrderNotFound" }));

        app.MapPost($"{Prefix}/{{orderId:long}}/ancillaries", async (long orderId, AddAncillariesRequest request, CancellationToken cancellationToken) =>
        {
            var (status, body) = await adds.AddAsync(orderId, request, cancellationToken);

            return Results.Json(body, statusCode: status);
        });

        app.MapPost($"{Prefix}/{{orderId:long}}/mutations", (long orderId, OrderMutationRequest request) =>
            store.Find(orderId) is { } order
                ? TestOrderMutations.Apply(order, request, clock) is { } error
                    ? Results.UnprocessableEntity(new { error })
                    : Results.Ok(TestOrderSnapshot.Of(order))
                : Results.NotFound(new { error = "OrderNotFound" }));

        await app.StartAsync();

        return new OrderingSimulator(app);
    }

    public async ValueTask DisposeAsync()
    {
        await _app.StopAsync();
        await _app.DisposeAsync();
    }
}
