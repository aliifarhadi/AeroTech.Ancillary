namespace AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Specifications
{
    public sealed class SpecificationReference
    {
        private SpecificationReference()
        {
        }

        internal SpecificationReference(long referenceId) => ReferenceId = referenceId;

        public long ReferenceId { get; private set; }
    }
}
