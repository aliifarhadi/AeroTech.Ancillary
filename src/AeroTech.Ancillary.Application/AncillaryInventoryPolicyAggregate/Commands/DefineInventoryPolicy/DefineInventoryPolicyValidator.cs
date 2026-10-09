using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy
{
    public abstract class DefineInventoryPolicyValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IDefineInventoryPolicyCommand
    {
        protected DefineInventoryPolicyValidator()
        {
            RuleFor(command => command.OwnerAirlineId).GreaterThan(0);
            RuleFor(command => command.ServiceDefinitionRef).NotEmpty().MaximumLength(30);
            RuleFor(command => command.ServiceDefinitionId).GreaterThan(0);
            RuleFor(command => command.Authority).IsInEnum();
            RuleFor(command => command.LocalPattern).IsInEnum().When(command => command.LocalPattern is not null);
            RuleFor(command => command.ProviderKey).MaximumLength(50);
            RuleFor(command => command.CountConsumption!).SetValidator(new FlightCountConsumptionInputValidator()).When(command => command.CountConsumption is not null);
            RuleFor(command => command.WeightConsumption!).SetValidator(new FlightWeightConsumptionInputValidator()).When(command => command.WeightConsumption is not null);
            RuleFor(command => command.SlotConsumption!).SetValidator(new AirportSlotConsumptionInputValidator()).When(command => command.SlotConsumption is not null);
            RuleForEach(command => command.PassengerUsageLimits).NotNull().SetValidator(new PassengerUsageLimitInputValidator());
        }
    }
}
