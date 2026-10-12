namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class VariantCodeAttribute : Attribute
    {
        public VariantCodeAttribute(string code) => Code = code;

        public string Code { get; }
    }
}
