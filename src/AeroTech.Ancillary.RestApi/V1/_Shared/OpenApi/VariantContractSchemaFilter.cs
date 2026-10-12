using System.Globalization;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using AeroTech.Ancillary.RestApi.V1.AncillaryServiceDefinitionAggregate.Requests;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AeroTech.Ancillary.RestApi.V1._Shared.OpenApi
{
    public sealed class VariantContractSchemaFilter : ISchemaFilter
    {
        private const string VariantCode = "variantCode";
        private const string Specification = "specification";
        private const string SpecificationSuffix = "SpecificationDto";

        private static readonly Dictionary<Type, string> Contracts = new()
        {
            [typeof(DefineServiceDefinitionRequest<>)] = "Define{0}Definition",
            [typeof(ChangeServiceDefinitionRequest<>)] = "Change{0}Definition",
            [typeof(ServiceDefinitionFormDto<>)] = "{0}DefinitionForm"
        };

        private static readonly Dictionary<Type, Type> SingleVariantSpecifications = new()
        {
            [typeof(UpgradeSpecificationInput)] = typeof(CabinUpgradeSpecificationDto),
            [typeof(PrioritySpecificationInput)] = typeof(PriorityBoardingCheckinSpecificationDto),
            [typeof(ConnectivitySpecificationInput)] = typeof(OnboardWifiSpecificationDto)
        };

        public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema is not OpenApiSchema contract
                || contract.Properties is null
                || !context.Type.IsGenericType
                || !Contracts.TryGetValue(context.Type.GetGenericTypeDefinition(), out var name))
                return;

            var written = context.Type.GetGenericArguments()[0];
            var documented = written.GetCustomAttribute<VariantSpecificationAttribute>();
            var family = documented?.Variants ?? SingleVariantSpecifications.GetValueOrDefault(written, written);
            var variants = family.GetCustomAttributes<JsonDerivedTypeAttribute>().Select(derived => derived.DerivedType).DefaultIfEmpty(family).ToList();

            if (variants.Count == 1)
            {
                contract.Properties[VariantCode] = CodeOf(variants[0]);

                return;
            }

            var alternatives = variants.ToDictionary(
                variant => variant.GetCustomAttribute<VariantCodeAttribute>()!.Code,
                variant => context.SchemaRepository.AddDefinition(
                    string.Format(CultureInfo.InvariantCulture, name, variant.Name[..^SpecificationSuffix.Length]),
                    AlternativeOf(contract, variant, context)));

            Forget(written, context);
            Forget(family, context);

            contract.Type = null;
            contract.Properties = null;
            contract.Required = null;
            contract.AdditionalPropertiesAllowed = true;
            contract.OneOf = [.. alternatives.Values];
            contract.Discriminator = new OpenApiDiscriminator { PropertyName = VariantCode, Mapping = alternatives };
            contract.Description = documented?.Compatibility;
        }

        private static OpenApiSchema AlternativeOf(OpenApiSchema contract, Type variant, SchemaFilterContext context)
        {
            var code = variant.GetCustomAttribute<VariantCodeAttribute>()!.Code;

            return new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                Description = $"{code} {AncillaryVariant.Find(code)!.Name}",
                Properties = new Dictionary<string, IOpenApiSchema>(contract.Properties!)
                {
                    [VariantCode] = CodeOf(variant),
                    [Specification] = context.SchemaGenerator.GenerateSchema(variant, context.SchemaRepository)
                },
                Required = contract.Required is null ? null : new HashSet<string>(contract.Required),
                AdditionalPropertiesAllowed = false
            };
        }

        private static OpenApiSchema CodeOf(Type variant)
            => new()
            {
                Type = JsonSchemaType.String,
                Enum = [JsonValue.Create(variant.GetCustomAttribute<VariantCodeAttribute>()!.Code)]
            };

        private static void Forget(Type type, SchemaFilterContext context)
        {
            if (context.SchemaRepository.TryLookupByType(type, out var reference) && reference.Reference.Id is { } id)
                context.SchemaRepository.Schemas.Remove(id);
        }
    }
}
