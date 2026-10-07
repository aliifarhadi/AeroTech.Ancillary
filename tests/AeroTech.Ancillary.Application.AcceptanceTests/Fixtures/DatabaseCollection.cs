using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;

[CollectionDefinition(Name)]
public sealed class DatabaseCollection : ICollectionFixture<TestDatabase>
{
    public const string Name = "Database";
}
