using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Fakes;

public sealed class SequentialIdGenerator : IIdGenerator
{
    private long _last = 1_000;

    public long NewId() => Interlocked.Increment(ref _last);
}
