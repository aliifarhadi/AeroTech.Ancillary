using AeroTech.Ancillary.Domain.AncillaryProductAggregate;
using AeroTech.Ancillary.Domain.AncillaryProductAggregate.ValueObjects;
using AeroTech.Ancillary.Domain.ServiceSubCodeAggregate;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;

public sealed record ProductSpec
{
    public static ProductSpec X => new();

    public static ProductSpec G => new() { ProductRef = "XBAGG", Name = "Extra bag 23kg", Rfisc = "XBG", Max = 2 };

    public static ProductSpec L => new()
    {
        ProductRef = "LNGTHR",
        Type = AncillaryProductType.LoungeAccess,
        Name = "Lounge access",
        SalesScope = AncillarySalesScope.TravellerSegment,
        Unit = AncillaryQuantityUnit.Each,
        DocumentType = AncillaryDocumentType.EmdStandalone,
        Rfisc = "0BX",
        ServiceTypeCode = "F",
        HasBaggage = false,
        LoungeAirportIds = [1]
    };

    public static ProductSpec LG => L with { ProductRef = "LNGXLG", Rfisc = SubCodes.LoungeCode, Max = 2, LoungeAirportIds = [1, 5] };

    public int OwnerAirlineId { get; init; } = SubCodes.AirlineId;

    public string ProductRef { get; init; } = "XBAG1";

    public AncillaryProductType Type { get; init; } = AncillaryProductType.ExtraBaggage;

    public string Name { get; init; } = "First extra bag 23kg";

    public string? Description { get; init; }

    public AncillarySalesScope SalesScope { get; init; } = AncillarySalesScope.TravellerBound;

    public AncillaryQuantityUnit Unit { get; init; } = AncillaryQuantityUnit.Piece;

    public int Min { get; init; } = 1;

    public int Max { get; init; } = 1;

    public AncillaryDocumentType DocumentType { get; init; } = AncillaryDocumentType.EmdAssociated;

    public string? Rfisc { get; init; } = "0CC";

    public string ServiceTypeCode { get; init; } = "C";

    public bool Refundable { get; init; }

    public bool? Commissionable { get; init; }

    public bool? Reusable { get; init; }

    public string? FormOfRefundCode { get; init; }

    public bool? InterlineSettlementAllowed { get; init; }

    public AncillaryInventoryControl InventoryControl { get; init; } = AncillaryInventoryControl.Unlimited;

    public bool HasBaggage { get; init; } = true;

    public int? Pieces { get; init; } = 1;

    public decimal? Weight { get; init; } = 23m;

    public AncillaryWeightUnit? WeightUnit { get; init; } = AncillaryWeightUnit.Kg;

    public IReadOnlyList<int>? LoungeAirportIds { get; init; }

    public AncillaryProduct Define(long id, ServiceSubCode? subCode, DateTimeOffset createdAt)
        => AncillaryProduct.Define(
            id,
            OwnerAirlineId,
            ProductRef,
            Type,
            Name,
            Description,
            SalesScope,
            new QuantityPolicy(Unit, Min, Max),
            DocumentType,
            Rfisc,
            ServiceTypeCode,
            Terms(),
            InventoryControl,
            Baggage(),
            Lounge(),
            subCode,
            createdAt);

    public void Change(AncillaryProduct product, ServiceSubCode? subCode)
        => product.Change(
            Name,
            Description,
            SalesScope,
            new QuantityPolicy(Unit, Min, Max),
            DocumentType,
            Rfisc,
            ServiceTypeCode,
            Terms(),
            InventoryControl,
            Baggage(),
            Lounge(),
            subCode);

    private SalesTerms Terms() => new(Refundable, Commissionable, Reusable, FormOfRefundCode, InterlineSettlementAllowed);

    private BaggageDetail? Baggage() => HasBaggage ? new BaggageDetail(Pieces, Weight, WeightUnit) : null;

    private LoungeDetail? Lounge() => LoungeAirportIds is null ? null : new LoungeDetail(LoungeAirportIds);
}
