namespace AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;

public static class RepositoryFiles
{
    private const string Solution = "AeroTech.Ancillary.sln";
    private const string GoldenExampleHeading = "### Golden example";
    private const string JsonFence = "```json";
    private const string Fence = "```";

    public static string Root { get; } = FindRoot();

    public static string TestProjectFile(string name)
        => Path.Combine(Root, "tests", typeof(RepositoryFiles).Assembly.GetName().Name!, name);

    public static (string Request, string ResponseData) GoldenExample()
    {
        var document = File.ReadAllText(Path.Combine(Root, "docs", "phases", "Phase-1-Extra-Baggage.md"));
        var example = document[document.IndexOf(GoldenExampleHeading, StringComparison.Ordinal)..];

        return (JsonBlockAfter(example, "Request:"), JsonBlockAfter(example, "Response `data`:"));
    }

    public static (string Request, string LoungeItem) LoungeGoldenExample()
    {
        var document = File.ReadAllText(Path.Combine(Root, "docs", "phases", "Phase-2-Lounge-Access.md"));
        var example = document[document.IndexOf(GoldenExampleHeading, StringComparison.Ordinal)..];

        return (JsonBlockAfter(example, "Request"), JsonBlockAfter(example, "The lounge item"));
    }

    public static string ProofRequestBody(string proofFile, int number)
    {
        var proof = File.ReadAllText(Path.Combine(Root, "docs", "proof", proofFile));
        var request = proof[proof.IndexOf($"### {number}. ", StringComparison.Ordinal)..];
        var start = request.IndexOf('{', request.IndexOf("Content-Type", StringComparison.Ordinal));
        var end = request.IndexOf("\n###", start, StringComparison.Ordinal);

        return end < 0 ? request[start..] : request[start..end];
    }

    private static string JsonBlockAfter(string text, string marker)
    {
        var start = text.IndexOf(JsonFence, text.IndexOf(marker, StringComparison.Ordinal), StringComparison.Ordinal) + JsonFence.Length;

        return text[start..text.IndexOf(Fence, start, StringComparison.Ordinal)];
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, Solution)))
            directory = directory.Parent;

        return directory?.FullName ?? throw new InvalidOperationException($"{Solution} was not found above the test output.");
    }
}
