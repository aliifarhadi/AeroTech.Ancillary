using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.ValueObjects;
using AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using static AeroTech.Ancillary.Domain.ConformanceTests.Fixtures.M1Fixtures;

namespace AeroTech.Ancillary.Domain.ConformanceTests;

public class M1ServiceDefinitionConformanceTests
{
    private static readonly ServiceDefinitionClassificationArgs NoClassification = new(null, null, null, null, null);

    [Fact]
    public void M1_C01_C02_a_draft_definition_carries_its_supplier_and_version_one()
    {
        var definition = LoungeDefinition();

        Assert.Equal(1001, definition.Id);
        Assert.Equal(2001, definition.SupplierId);
        Assert.Equal("LNG_IKA_CIP", definition.ServiceDefinitionRef);
        Assert.Equal(1, definition.Version);
        Assert.Equal(ServiceDefinitionStatus.Draft, definition.Status);
        Assert.Null(definition.ActivatedAt);
    }

    [Fact]
    public void M1_C01_an_industry_sub_code_resolves_its_classification_from_the_reference()
    {
        var definition = LoungeDefinition();

        Assert.Equal("F", definition.ServiceTypeCode);
        Assert.Equal("0BX", definition.ServiceSubCode);
        Assert.Equal(ServiceSubCodeSource.Industry, definition.SubCodeSource);
        Assert.Equal("LG", definition.GroupCode);
        Assert.Null(definition.SubGroupCode);
        Assert.Equal(AncillaryDocumentType.EmdStandalone, definition.Document.Type);
        Assert.Equal("E", definition.Document.Rfic);
        Assert.Equal("0BX", definition.Document.Rfisc);
    }

    [Fact]
    public void M1_C01_an_unknown_industry_sub_code_is_refused()
    {
        BusinessAssert.Throws(16207, 422, () => AncillaryServiceDefinition.Define(
            1002, Airline, 2001, "LNG_X", 1, "0ZZ", ServiceSubCodeSource.Industry, NoClassification, V122Fixtures.Profile(PricingUnit.PerPassenger, ServiceDateBasis.FlightDeparture, AncillaryDocumentType.EmdStandalone), PricingUnit.PerPassenger, ServiceDateBasis.FlightDeparture,
            "Lounge access", null,
            DocumentDefinition.Create(AncillaryDocumentType.EmdStandalone, "E", "0ZZ"),
            BookingDefinition.Create(BookingMethod.NoBookingProcessRequired, null, null),
            null, null, Now));
    }

    [Fact]
    public void M1_C01_industry_semantics_cannot_be_redefined()
    {
        BusinessAssert.Throws(16208, 422, () => AncillaryServiceDefinition.Define(
            1002, Airline, 2001, "LNG_X", 1, "0BX", ServiceSubCodeSource.Industry,
            new ServiceDefinitionClassificationArgs("C", null, null, null, null),
            V122Fixtures.Profile(PricingUnit.PerPassenger, ServiceDateBasis.FlightDeparture, AncillaryDocumentType.EmdStandalone),
            PricingUnit.PerPassenger,
            ServiceDateBasis.FlightDeparture,
            "Lounge access", null,
            DocumentDefinition.Create(AncillaryDocumentType.EmdStandalone, "E", "0BX"),
            BookingDefinition.Create(BookingMethod.NoBookingProcessRequired, null, null),
            null, null, Now));

        BusinessAssert.Throws(16208, 422, () => AncillaryServiceDefinition.Define(
            1002, Airline, 2001, "LNG_X", 1, "0BX", ServiceSubCodeSource.Industry, NoClassification, V122Fixtures.Profile(PricingUnit.PerPassenger, ServiceDateBasis.FlightDeparture, AncillaryDocumentType.EmdAssociated), PricingUnit.PerPassenger, ServiceDateBasis.FlightDeparture,
            "Lounge access", null,
            DocumentDefinition.Create(AncillaryDocumentType.EmdAssociated, "E", "0BX"),
            BookingDefinition.Create(BookingMethod.NoBookingProcessRequired, null, null),
            null, null, Now));

        BusinessAssert.Throws(16208, 422, () => AncillaryServiceDefinition.Define(
            1002, Airline, 2001, "LNG_X", 1, "0BX", ServiceSubCodeSource.Industry, NoClassification, V122Fixtures.Profile(PricingUnit.PerPassenger, ServiceDateBasis.FlightDeparture, AncillaryDocumentType.EmdStandalone), PricingUnit.PerPassenger, ServiceDateBasis.FlightDeparture,
            "Lounge access", null,
            DocumentDefinition.Create(AncillaryDocumentType.EmdStandalone, "C", "0BX"),
            BookingDefinition.Create(BookingMethod.NoBookingProcessRequired, null, null),
            null, null, Now));

        BusinessAssert.Throws(16208, 422, () => AncillaryServiceDefinition.Define(
            1002, Airline, 2001, "LNG_X", 1, "0BX", ServiceSubCodeSource.Industry, NoClassification, V122Fixtures.Profile(PricingUnit.PerPassenger, ServiceDateBasis.FlightDeparture, AncillaryDocumentType.EmdStandalone), PricingUnit.PerPassenger, ServiceDateBasis.FlightDeparture,
            "Lounge access", null,
            DocumentDefinition.Create(AncillaryDocumentType.EmdStandalone, "E", "0CC"),
            BookingDefinition.Create(BookingMethod.NoBookingProcessRequired, null, null),
            null, null, Now));
    }

    [Fact]
    public void M1_C01_a_carrier_defined_sub_code_supplies_its_own_classification()
    {
        var definition = AncillaryServiceDefinition.Define(
            1002, Airline, 2001, "LNG_OWN", 1, "XLG", ServiceSubCodeSource.CarrierDefined,
            new ServiceDefinitionClassificationArgs("F", "LG", null, null, null),
            V122Fixtures.Profile(PricingUnit.PerPassenger, ServiceDateBasis.FlightDeparture, AncillaryDocumentType.EmdStandalone),
            PricingUnit.PerPassenger,
            ServiceDateBasis.FlightDeparture,
            "Own lounge", null,
            DocumentDefinition.Create(AncillaryDocumentType.EmdStandalone, "E", "XLG"),
            BookingDefinition.Create(BookingMethod.NoBookingProcessRequired, null, null),
            null, null, Now);

        Assert.Equal("F", definition.ServiceTypeCode);
        Assert.Equal("LG", definition.GroupCode);

        BusinessAssert.Throws(16202, 422, () => AncillaryServiceDefinition.Define(
            1003, Airline, 2001, "LNG_OWN2", 1, "XLG", ServiceSubCodeSource.CarrierDefined,
            new ServiceDefinitionClassificationArgs("Q", "LG", null, null, null),
            V122Fixtures.Profile(PricingUnit.PerPassenger, ServiceDateBasis.FlightDeparture, AncillaryDocumentType.EmdStandalone),
            PricingUnit.PerPassenger,
            ServiceDateBasis.FlightDeparture,
            "Own lounge", null,
            DocumentDefinition.Create(AncillaryDocumentType.EmdStandalone, "E", "XLG"),
            BookingDefinition.Create(BookingMethod.NoBookingProcessRequired, null, null),
            null, null, Now));

        BusinessAssert.Throws(16202, 422, () => AncillaryServiceDefinition.Define(
            1003, Airline, 2001, "LNG_OWN2", 1, "XLG", ServiceSubCodeSource.CarrierDefined,
            new ServiceDefinitionClassificationArgs("F", null, null, null, null),
            V122Fixtures.Profile(PricingUnit.PerPassenger, ServiceDateBasis.FlightDeparture, AncillaryDocumentType.EmdStandalone),
            PricingUnit.PerPassenger,
            ServiceDateBasis.FlightDeparture,
            "Own lounge", null,
            DocumentDefinition.Create(AncillaryDocumentType.EmdStandalone, "E", "XLG"),
            BookingDefinition.Create(BookingMethod.NoBookingProcessRequired, null, null),
            null, null, Now));
    }

    [Fact]
    public void M1_C01_the_reference_charsets_are_enforced()
    {
        BusinessAssert.Throws(16202, 422, () => LoungeDefinition(reference: "lng ika"));
        BusinessAssert.Throws(16202, 422, () => LoungeDefinition(reference: new string('R', 31)));
        BusinessAssert.Throws(16202, 422, () => AncillaryServiceDefinition.Define(
            1002, Airline, 0, "LNG_X", 1, "0BX", ServiceSubCodeSource.Industry, NoClassification, V122Fixtures.Profile(PricingUnit.PerPassenger, ServiceDateBasis.FlightDeparture, AncillaryDocumentType.EmdStandalone), PricingUnit.PerPassenger, ServiceDateBasis.FlightDeparture,
            "Lounge access", null,
            DocumentDefinition.Create(AncillaryDocumentType.EmdStandalone, "E", "0BX"),
            BookingDefinition.Create(BookingMethod.NoBookingProcessRequired, null, null),
            null, null, Now));
    }

    [Fact]
    public void M1_C01_an_ssr_booking_requires_its_ssr_code()
    {
        BusinessAssert.Throws(16202, 422, () => BookingDefinition.Create(BookingMethod.Ssr, null, null));

        var booking = BookingDefinition.Create(BookingMethod.Ssr, "LNGS", null);

        Assert.Equal("LNGS", booking.SsrCode);
    }

    [Fact]
    public void M1_C04_activation_needs_a_draft_and_the_matching_active_supplier()
    {
        var supplier = LocalSupplier();
        var definition = LoungeDefinition();

        definition.Activate(supplier, Now.AddMinutes(1));

        Assert.Equal(ServiceDefinitionStatus.Active, definition.Status);
        Assert.Equal(Now.AddMinutes(1), definition.ActivatedAt);

        BusinessAssert.Throws(16203, 409, () => definition.Activate(supplier, Now.AddMinutes(2)));

        var other = LocalSupplier(id: 2008);
        var second = LoungeDefinition(id: 1002, reference: "LNG_IKA_2");

        BusinessAssert.Throws(16202, 422, () => second.Activate(other, Now.AddMinutes(1)));
    }

    [Fact]
    public void M1_C09_M05_the_ref_and_version_stay_for_display_and_never_resolve_fulfillment()
    {
        var definition = LoungeDefinition();

        Assert.Equal("LNG_IKA_CIP", definition.ServiceDefinitionRef);
        Assert.Equal(1, definition.Version);

        var resolvers = typeof(IAncillaryServiceDefinitionRepository).GetMethods()
            .Where(method => method.ReturnType.GenericTypeArguments.Contains(typeof(AncillaryServiceDefinition)))
            .ToList();

        var resolver = Assert.Single(resolvers);
        Assert.Equal(typeof(long), resolver.GetParameters()[0].ParameterType);
    }
}
