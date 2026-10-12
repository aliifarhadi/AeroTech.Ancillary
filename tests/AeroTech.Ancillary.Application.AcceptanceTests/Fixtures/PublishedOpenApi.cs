using AeroTech.Ancillary.RestApi;
using AeroTech.Ancillary.RestApi.V1._Shared.OpenApi;
using AeroTech.Framework.Presentation.Extensions;
using AeroTech.Framework.Presentation.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using NJsonSchema;
using Swashbuckle.AspNetCore.Swagger;
using PublishedDocument = NSwag.OpenApiDocument;
using PublishedOperation = NSwag.OpenApiOperation;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;

public static class PublishedOpenApi
{
    private const string Json = "application/json";
    private const string Ok = "200";

    private static readonly Lazy<Task<string>> Text = new(WriteAsync);
    private static readonly Lazy<Task<PublishedDocument>> Published = new(async () => await PublishedDocument.FromJsonAsync(await Text.Value));

    public static Task<string> TextAsync() => Text.Value;

    public static Task<PublishedDocument> DocumentAsync() => Published.Value;

    public static async Task<PublishedOperation> OperationAsync(string method, string path)
    {
        var document = await DocumentAsync();

        return document.Paths[path][method];
    }

    public static async Task<JsonSchema> RequestSchemaAsync(string method, string path)
        => (await OperationAsync(method, path)).RequestBody.Content[Json].Schema.ActualSchema;

    public static async Task<JsonSchema> AnswerSchemaAsync(string method, string path)
        => (await OperationAsync(method, path)).Responses[Ok].Content[Json].Schema.ActualSchema.Properties["data"].ActualSchema;

    public static string[] Errors(JsonSchema schema, string json)
        => schema.Validate(json, SchemaType.OpenApi3).Select(error => $"{error.Path} {error.Kind}").ToArray();

    private static async Task<string> WriteAsync()
    {
        var builder = WebApplication.CreateBuilder();

        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            [$"{JwtOptions.SectionName}:{nameof(JwtOptions.Authority)}"] = "https://identity.invalid/",
            [$"{JwtOptions.SectionName}:{nameof(JwtOptions.Audience)}"] = "pss-api"
        });
        builder.Services
            .AddPresentation(builder.Configuration, typeof(RestApiAssembly).Assembly)
            .AddContractDocumentation();

        await using var application = builder.Build();

        var document = application.Services.GetRequiredService<ISwaggerProvider>().GetSwagger("v1");

        return await document.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0);
    }
}
