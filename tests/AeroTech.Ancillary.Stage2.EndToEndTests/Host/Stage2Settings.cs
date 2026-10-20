namespace AeroTech.Ancillary.Stage2.EndToEndTests.Host;

public static class Stage2Settings
{
    public const string Environment = "E2E";
    public const string SigningKey = "stage2-e2e-priced-selection-signing-key-0123456789";

    public static readonly Uri AirAvailBaseAddress = new("https://airavail.e2e.invalid/service/");

    public static readonly IReadOnlyDictionary<string, string> Values = new Dictionary<string, string>
    {
        ["ConnectionStrings:CommandDbContext"] = "Server=127.0.0.1,1;Database=Stage2E2EBlocked;User Id=e2e;Password=e2e;TrustServerCertificate=True;Connect Timeout=1",
        ["ConnectionStrings:QueryDbContext"] = "Server=127.0.0.1,1;Database=Stage2E2EBlocked;User Id=e2e;Password=e2e;TrustServerCertificate=True;Connect Timeout=1",
        ["Redis:Host"] = "127.0.0.1:1,connectTimeout=100,connectRetry=0",
        ["RabbitMq:Server"] = "rabbitmq.e2e.invalid",
        ["RabbitMq:UserName"] = "e2e",
        ["RabbitMq:Password"] = "e2e",
        ["Jwt:Authority"] = "https://identity.e2e.invalid",
        ["Jwt:Audience"] = "ancillary-e2e",
        ["Offer:BaseUrl"] = AirAvailBaseAddress.ToString(),
        ["Shopping:SelectionSigningKey"] = SigningKey,
        ["Shopping:SourceContextTtl"] = "00:30:00",
        ["IdGenerator:GeneratorId"] = "0"
    };
}
