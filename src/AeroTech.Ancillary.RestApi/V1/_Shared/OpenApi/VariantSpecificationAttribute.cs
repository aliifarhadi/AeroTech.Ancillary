namespace AeroTech.Ancillary.RestApi.V1._Shared.OpenApi
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class VariantSpecificationAttribute : Attribute
    {
        public VariantSpecificationAttribute(Type variants, string compatibility)
        {
            Variants = variants;
            Compatibility = compatibility;
        }

        public Type Variants { get; }

        public string Compatibility { get; }
    }
}
