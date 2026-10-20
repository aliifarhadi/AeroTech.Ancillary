using System.Globalization;
using AeroTech.Ancillary.Application.Orders;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.Common.Orders
{
    public static class OrderShoppingContextMapper
    {
        public const string Provenance = "Ordering/CommercialContext";

        public static AncillaryShoppingContext Map(AncillaryShoppingContext context, OrderCommercialContext order)
        {
            var travellers = order.Travellers.ToDictionary(traveller => traveller.TravellerRef, StringComparer.Ordinal);

            return context with
            {
                ShoppingStage = order.TicketedAtUtc is null ? ShoppingStage.PreOrder : ShoppingStage.PostTicketed,
                SourceIdentity = context.SourceIdentity with
                {
                    SourceKind = ShoppingSourceKind.Order,
                    TrustedSourceReference = order.OrderId.ToString(CultureInfo.InvariantCulture),
                    OrderId = order.OrderId,
                    OrderCommercialVersion = order.CommercialVersion,
                    TicketedAtUtc = order.TicketedAtUtc?.ToUniversalTime()
                },
                Travellers = context.Travellers
                    .Select(traveller => travellers.TryGetValue(traveller.TravellerRef, out var facts)
                        ? traveller with { OrderTravellerId = facts.OrderTravellerId, DateOfBirth = facts.DateOfBirth, AssociatedAdultRef = facts.InfantOfTravellerRef }
                        : traveller)
                    .ToList(),
                ExistingServiceFacts = order.Services
                    .Select(service => new ExistingAncillaryServiceFacts
                    {
                        ServiceRef = service.ServiceRef,
                        OrderServiceId = service.OrderServiceId,
                        TravellerRef = service.TravellerRef,
                        PortionRef = service.PortionRef,
                        FlightRefs = service.FlightRefs,
                        Quantity = service.Quantity,
                        CommercialState = service.CommercialState,
                        EvidenceSourceRef = Provenance
                    })
                    .ToList(),
                CoverageCompleteness = context.CoverageCompleteness with { ExistingServicesComplete = true, Provenance = Provenance }
            };
        }
    }
}
