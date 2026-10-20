using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AeroTech.Ancillary.Stage2.EndToEndTests.Host;

public sealed class BlockedSqlInterceptor(IsolationGuard guard, string contextName) : DbConnectionInterceptor
{
    public override InterceptionResult ConnectionOpening(DbConnection connection, ConnectionEventData eventData, InterceptionResult result)
        => throw Blocked();

    public override ValueTask<InterceptionResult> ConnectionOpeningAsync(
        DbConnection connection,
        ConnectionEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default)
        => throw Blocked();

    private InvalidOperationException Blocked()
    {
        guard.Record($"SQL {contextName}: connection opening");

        return new InvalidOperationException($"E2E isolation: '{contextName}' tried to open a SQL Server connection.");
    }
}
