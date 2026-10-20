using System.Text.Json.Nodes;

namespace AeroTech.Ancillary.Stage2.EndToEndTests.Scenario;

public sealed record BatchView(int Status, IReadOnlyList<BatchLine> Lines, bool AllReady, IReadOnlyList<string> BasketReasonCodes, decimal? Total, JsonNode? Body)
{
    public IReadOnlyList<string> Refs => Lines.Select(line => line.PricedSelectionRef).OfType<string>().ToList();

    public BatchLine Line(string clientSelectionId) => Lines.Single(line => line.ClientSelectionId == clientSelectionId);

    public static BatchView Of(string surface, E2EResponse response)
    {
        if (response.Status != 200)
            return new BatchView(response.Status, [], false, [], null, response.Body);

        var batch = surface == Stage2Flows.Api ? response.Data["data"]! : response.Data;
        var basket = batch["basket"]!;

        return new BatchView(
            response.Status,
            batch["lines"]!.AsArray()
                .Select(line => new BatchLine(
                    line!["clientSelectionId"]!.GetValue<string>(),
                    line["status"] is JsonObject status ? status["name"]!.GetValue<string>() : line["status"]!.GetValue<string>(),
                    Stage2Json.Strings(line["reasonCodes"]),
                    line["canPurchase"]!.GetValue<bool>(),
                    line["pricedSelectionRef"]?.GetValue<string>()))
                .ToList(),
            basket["allReady"]!.GetValue<bool>(),
            Stage2Json.Strings(basket["reasonCodes"]),
            basket["total"]?.GetValue<decimal>(),
            response.Body);
    }
}
