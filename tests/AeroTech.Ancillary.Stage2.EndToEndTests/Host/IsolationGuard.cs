using System.Collections.Concurrent;

namespace AeroTech.Ancillary.Stage2.EndToEndTests.Host;

public sealed class IsolationGuard
{
    private readonly ConcurrentQueue<string> _escapes = new();

    public IReadOnlyList<string> Escapes => _escapes.ToList();

    public void Record(string escape) => _escapes.Enqueue(escape);
}
