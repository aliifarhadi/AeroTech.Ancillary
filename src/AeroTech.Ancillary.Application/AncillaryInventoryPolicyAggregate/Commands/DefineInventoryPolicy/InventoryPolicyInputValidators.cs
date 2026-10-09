using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy
{
    public sealed class FlightCountConsumptionInputValidator : AbstractValidator<FlightCountConsumptionInput>
    {
        public FlightCountConsumptionInputValidator()
        {
            RuleFor(count => count.ResourceId).GreaterThan(0);
            RuleFor(count => count.CountUnit).IsInEnum();
        }
    }

    public sealed class FlightWeightConsumptionInputValidator : AbstractValidator<FlightWeightConsumptionInput>
    {
        public FlightWeightConsumptionInputValidator()
        {
            RuleFor(weight => weight.WeightResourceId).GreaterThan(0);
            RuleFor(weight => weight.ConsumptionMode).IsInEnum();
        }
    }

    public sealed class AirportSlotConsumptionInputValidator : AbstractValidator<AirportSlotConsumptionInput>
    {
        public AirportSlotConsumptionInputValidator()
        {
            RuleFor(slot => slot.FacilityId).GreaterThan(0);
        }
    }

    public sealed class PassengerUsageLimitInputValidator : AbstractValidator<PassengerUsageLimitInput>
    {
        public PassengerUsageLimitInputValidator()
        {
            RuleFor(limit => limit.LimitScope).IsInEnum();
            RuleFor(limit => limit.CountingFamilyCode).NotEmpty().MaximumLength(30);
            RuleFor(limit => limit.ConsumptionUnit).IsInEnum();
        }
    }
}
