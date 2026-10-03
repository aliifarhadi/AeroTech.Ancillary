using System.Globalization;
using System.Text.RegularExpressions;

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
        var document = File.ReadAllText(Path.Combine(Root, "docs", "phases", "P1-Extra-Baggage", "phase.md"));
        var example = document[document.IndexOf(GoldenExampleHeading, StringComparison.Ordinal)..];

        return (JsonBlockAfter(example, "Request:"), JsonBlockAfter(example, "Response `data`:"));
    }

    public static (string Request, string LoungeItem) LoungeGoldenExample()
    {
        var document = File.ReadAllText(Path.Combine(Root, "docs", "phases", "P2-Lounge-Access", "phase.md"));
        var example = document[document.IndexOf(GoldenExampleHeading, StringComparison.Ordinal)..];

        return (JsonBlockAfter(example, "Request"), JsonBlockAfter(example, "The lounge item"));
    }

    public static string ProofRequestBody(string phaseFolder, int number)
    {
        var proof = File.ReadAllText(Path.Combine(Root, "docs", "phases", phaseFolder, "proof.http"));
        var request = proof[proof.IndexOf($"### {number}. ", StringComparison.Ordinal)..];
        var start = request.IndexOf('{', request.IndexOf("Content-Type", StringComparison.Ordinal));
        var end = request.IndexOf("\n###", start, StringComparison.Ordinal);

        return end < 0 ? request[start..] : request[start..end];
    }

    public static IReadOnlyList<ProofRequest> ProofRequests(string phaseFolder)
    {
        var lines = File.ReadAllLines(Path.Combine(Root, "docs", "phases", phaseFolder, "proof.http"));
        var variables = lines
            .Select(line => Regex.Match(line, @"^@(\w+)\s*=\s*(.+)$"))
            .Where(match => match.Success)
            .ToDictionary(match => match.Groups[1].Value, match => match.Groups[2].Value.Trim());
        var requests = new List<ProofRequest>();

        for (var index = 0; index < lines.Length; index++)
        {
            var heading = Regex.Match(lines[index], @"^### (\d+)\. ");

            if (!heading.Success)
                continue;

            var text = new List<string>();
            var cursor = index;

            while (cursor < lines.Length && lines[cursor].StartsWith("###", StringComparison.Ordinal))
                text.Add(lines[cursor++]);

            while (cursor < lines.Length && !Regex.IsMatch(lines[cursor], "^(GET|POST|PUT|DELETE) "))
                cursor++;

            if (cursor == lines.Length)
                break;

            var requestLine = lines[cursor++].Split(' ', 2);

            while (cursor < lines.Length && !lines[cursor].StartsWith('{') && !lines[cursor].StartsWith("###", StringComparison.Ordinal))
                cursor++;

            var body = new List<string>();

            while (cursor < lines.Length && !lines[cursor].StartsWith("###", StringComparison.Ordinal))
                body.Add(lines[cursor++]);

            requests.Add(new ProofRequest(
                int.Parse(heading.Groups[1].Value, CultureInfo.InvariantCulture),
                string.Join(' ', text),
                requestLine[0],
                Resolve(requestLine[1], variables),
                body.Count == 0 ? null : Resolve(string.Join('\n', body), variables)));
        }

        return requests;
    }

    private static string Resolve(string text, IReadOnlyDictionary<string, string> variables)
        => Regex.Replace(text, @"\{\{([^}]+)\}\}", match => variables.GetValueOrDefault(match.Groups[1].Value) ?? "1");

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

public sealed record ProofRequest(int Number, string Heading, string Method, string Route, string? Body);
