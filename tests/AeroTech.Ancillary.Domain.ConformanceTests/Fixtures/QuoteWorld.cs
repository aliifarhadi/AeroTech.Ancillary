using AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate;
using AeroTech.Ancillary.Domain.AncillaryProductAggregate;
using AeroTech.Ancillary.Domain.AncillaryQuote;
using AeroTech.Ancillary.Domain.ConformanceTests.Fakes;

namespace AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;

public sealed class QuoteWorld
{
    private readonly SequentialIdGenerator _ids = new();
    private readonly List<AncillaryProduct> _products = [];
    private readonly List<AncillaryPriceRule> _rules = [];

    public DateTimeOffset Clock { get; set; } = new(2031, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public IReadOnlyList<AncillaryProduct> Products => _products;

    public IReadOnlyList<AncillaryPriceRule> Rules => _rules;

    public AncillaryProduct AddDraftProduct(ProductSpec spec)
    {
        var product = spec.Define(_ids.NewId(), SubCodes.For(spec.Rfisc!, spec.OwnerAirlineId), Clock);

        _products.Add(product);

        return product;
    }

    public AncillaryProduct AddProduct(ProductSpec spec)
    {
        var product = AddDraftProduct(spec);

        product.Activate(SubCodes.For(spec.Rfisc!, spec.OwnerAirlineId), Clock);

        return product;
    }

    public AncillaryProduct AddRevision(AncillaryProduct source, ProductSpec spec)
    {
        var subCode = SubCodes.For(spec.Rfisc!, spec.OwnerAirlineId);
        var revision = source.Revise(_ids.NewId(), source.Version + 1, Clock);

        spec.Change(revision, subCode);
        revision.Activate(subCode, Clock);
        source.Retire(Clock);
        _products.Add(revision);

        return revision;
    }

    public AncillaryPriceRule AddDraftRule(RuleSpec spec)
    {
        var rule = spec.Define(_ids, Clock);

        _rules.Add(rule);

        return rule;
    }

    public AncillaryPriceRule AddRule(RuleSpec spec)
    {
        var rule = AddDraftRule(spec);

        rule.Activate();

        return rule;
    }

    public AncillaryQuoteResult Quote(AncillaryQuoteRequest request)
        => AncillaryQuoteEvaluator.Evaluate(request, _products, _rules);
}
