namespace AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Specifications
{
    public sealed class SpecificationCode
    {
        private SpecificationCode()
        {
        }

        internal SpecificationCode(string code) => Code = code;

        public string Code { get; private set; } = default!;
    }
}
