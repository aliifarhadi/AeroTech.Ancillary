using System.Diagnostics;
using System.Text.Json;

namespace AeroTech.Ancillary.Stage2.EndToEndTests.Scenario;

public sealed class Transcript
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly List<TranscriptEntry> _entries = [];

    public IReadOnlyList<TranscriptEntry> Entries
    {
        get
        {
            lock (_entries)
                return _entries.ToList();
        }
    }

    public DelegatingHandler Recorder(string caller) => new RecordingHandler(this, caller);

    public void Write(string testName, string outcome)
    {
        var directory = Path.Combine(AppContext.BaseDirectory, "e2e-transcripts");

        Directory.CreateDirectory(directory);
        File.WriteAllText(
            Path.Combine(directory, $"{testName}.json"),
            JsonSerializer.Serialize(new { test = testName, outcome, entries = Entries }, Json));
    }

    private void Add(TranscriptEntry entry)
    {
        lock (_entries)
            _entries.Add(entry with { Sequence = _entries.Count + 1 });
    }

    private sealed class RecordingHandler(Transcript transcript, string caller) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var requestBody = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            var watch = Stopwatch.StartNew();
            var response = await base.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            transcript.Add(new TranscriptEntry(
                0,
                caller,
                request.Method.Method,
                request.RequestUri?.PathAndQuery ?? string.Empty,
                requestBody,
                (int)response.StatusCode,
                responseBody,
                watch.Elapsed.TotalMilliseconds));

            return response;
        }
    }
}
