using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Framework.Core.Domain.Aggregates;
using Xunit;

namespace AeroTech.Ancillary.Domain.ConformanceTests;

public class P1BoundaryConformanceTests
{
    private static readonly Type[] DomainTypes = typeof(AncillaryProvision).Assembly.GetTypes();

    [Fact]
    public void P1_A02_the_domain_has_no_evaluation_namespace_or_runtime_matching_type()
    {
        string[] forbidden =
        [
            "Evaluat", "Matcher", "RuleEngine", "FlightContext", "FareContext", "Simulat", "Shopping", "Offer", "Quote"
        ];

        Assert.DoesNotContain(DomainTypes, type => type.Namespace is not null && type.Namespace.Contains("Evaluation", StringComparison.Ordinal));

        foreach (var term in forbidden)
            Assert.DoesNotContain(DomainTypes, type => type.Name.Contains(term, StringComparison.Ordinal));
    }

    [Fact]
    public void P1_REQ_no_family_specific_aggregate_or_new_operational_concept_exists()
    {
        var aggregates = DomainTypes
            .Where(type => type is { IsClass: true, IsAbstract: false } && typeof(AggregateRoot<long>).IsAssignableFrom(type))
            .Select(type => type.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            new[] { "AncillaryPricing", "AncillaryProvision", "AncillaryReservation", "AncillaryServiceDefinition", "Supplier" },
            aggregates);

        string[] forbidden =
        [
            "StockPool", "Quota", "Adapter", "Wheelchair", "Meal", "Insurance", "Lounge", "Pet", "Wifi", "FastTrack",
            "PriorityBoarding", "Unaccompanied", "History", "Snapshot", "Split", "Issue", "Cancel", "Release"
        ];

        foreach (var term in forbidden)
            Assert.DoesNotContain(
                DomainTypes,
                type => type.Name.Contains(term, StringComparison.Ordinal) && !type.Name.Contains("ReadModelSnapshot", StringComparison.Ordinal));
    }
}
