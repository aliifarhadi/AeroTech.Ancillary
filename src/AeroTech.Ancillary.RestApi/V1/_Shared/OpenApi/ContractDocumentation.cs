using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AeroTech.Ancillary.RestApi.V1._Shared.OpenApi
{
    public static class ContractDocumentation
    {
        private const string LongFormat = "int64";

        public static IServiceCollection AddContractDocumentation(this IServiceCollection services)
            => services.Configure<SwaggerGenOptions>(options =>
            {
                options.SupportNonNullableReferenceTypes();
                options.NonNullableReferenceTypesAsRequired();
                options.UseOneOfForPolymorphism();
                options.MapType<long>(() => new OpenApiSchema { Type = JsonSchemaType.String, Format = LongFormat });
                options.MapType<long?>(() => new OpenApiSchema { Type = JsonSchemaType.String | JsonSchemaType.Null, Format = LongFormat });
                options.SchemaFilter<VariantSpecificationSchemaFilter>();
            });
    }
}
