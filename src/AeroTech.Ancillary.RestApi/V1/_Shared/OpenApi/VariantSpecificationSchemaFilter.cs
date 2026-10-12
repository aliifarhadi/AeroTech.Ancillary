using System.Reflection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AeroTech.Ancillary.RestApi.V1._Shared.OpenApi
{
    public sealed class VariantSpecificationSchemaFilter : ISchemaFilter
    {
        public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
        {
            var documented = context.Type.GetCustomAttribute<VariantSpecificationAttribute>();

            if (documented is null || schema is not OpenApiSchema written)
                return;

            var variants = context.SchemaGenerator.GenerateSchema(documented.Variants, context.SchemaRepository);

            written.Type = null;
            written.Properties = null;
            written.Required = null;
            written.AdditionalPropertiesAllowed = true;
            written.OneOf = variants.OneOf?.ToList();
            written.Description = documented.Compatibility;
        }
    }
}
