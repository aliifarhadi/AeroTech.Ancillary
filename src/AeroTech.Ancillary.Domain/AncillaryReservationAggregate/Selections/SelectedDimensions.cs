using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Specifications;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.ValueObjects;

namespace AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Selections
{
    public sealed class SelectedDimensions : ValueObject
    {
        private SelectedDimensions()
        {
        }

        private SelectedDimensions(decimal lengthCm, decimal widthCm, decimal heightCm)
        {
            LengthCm = lengthCm;
            WidthCm = widthCm;
            HeightCm = heightCm;
        }

        public decimal LengthCm { get; private set; }

        public decimal WidthCm { get; private set; }

        public decimal HeightCm { get; private set; }

        internal static SelectedDimensions Create(SelectedDimensionsArgs args, string owner)
        {
            if (!SpecificationRules.IsMeasure(args.LengthCm) || !SpecificationRules.IsMeasure(args.WidthCm) || !SpecificationRules.IsMeasure(args.HeightCm))
                throw ExceptionFactory.AncillaryHoldRequestIsInvalid($"{owner}.{nameof(SelectedDimensions)}");

            return new SelectedDimensions(args.LengthCm, args.WidthCm, args.HeightCm);
        }

        public SelectedDimensionsArgs ToArgs() => new(LengthCm, WidthCm, HeightCm);

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return LengthCm;
            yield return WidthCm;
            yield return HeightCm;
        }
    }
}
