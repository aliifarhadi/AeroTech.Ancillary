using System.Reflection;
using System.Text.Json;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct.Backoffice;
using AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Commands.RegisterServiceSubCode.Backoffice;
using AeroTech.Ancillary.Query.AncillaryQuote.Queries.GetAncillaryQuote.Service;
using FluentValidation;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Contracts;

public sealed class Phase2BRegressionContractTests
{
    [Fact]
    public void P2B_G01_NoPhaseOneOrPhaseTwoTestIsSkipped()
    {
        var tests = typeof(Phase2BRegressionContractTests).Assembly
            .GetTypes()
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .Where(method => method.Name.StartsWith("P1_", StringComparison.Ordinal) || method.Name.StartsWith("P2_", StringComparison.Ordinal))
            .ToList();

        Assert.Contains(tests, method => method.Name.StartsWith("P1_", StringComparison.Ordinal));
        Assert.Contains(tests, method => method.Name.StartsWith("P2_", StringComparison.Ordinal));
        Assert.All(tests, method => Assert.Null(Assert.Single(method.GetCustomAttributes<FactAttribute>()).Skip));
    }

    [Theory]
    [InlineData("P1-Extra-Baggage")]
    [InlineData("P2-Lounge-Access")]
    public void P2B_G02_ProofRequestsStillMatchTheContracts(string phaseFolder)
    {
        var checkedRequests = 0;

        foreach (var request in RepositoryFiles.ProofRequests(phaseFolder).Where(request => request is { Method: "POST", Body: not null }))
        {
            bool? accepted = request.Route.Split('/')[^1] switch
            {
                "ServiceSubCodes" => Accepts(request.Body!, new BackofficeRegisterServiceSubCodeCommandValidator()),
                "AncillaryProducts" => Accepts(request.Body!, new BackofficeDefineAncillaryProductCommandValidator()),
                "AncillaryPriceRules" => Accepts(request.Body!, new BackofficeDefineAncillaryPriceRuleCommandValidator()),
                "AncillaryQuotes" => Accepts(request.Body!, new ServiceGetAncillaryQuoteQueryValidator()),
                _ => null
            };

            if (accepted is null)
                continue;

            checkedRequests++;

            Assert.True(
                accepted != request.Heading.Contains("expect 400", StringComparison.Ordinal),
                $"{phaseFolder} request {request.Number}: {request.Heading}");
        }

        Assert.True(checkedRequests >= 15, $"{phaseFolder}: only {checkedRequests} requests were checked.");
    }

    private static bool Accepts<T>(string body, IValidator<T> validator)
    {
        try
        {
            return validator.Validate(ApiJson.Deserialize<T>(body)).IsValid;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
