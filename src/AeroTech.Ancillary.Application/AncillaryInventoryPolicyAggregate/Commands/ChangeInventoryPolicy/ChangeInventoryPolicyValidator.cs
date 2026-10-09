using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy;
using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.ChangeInventoryPolicy
{
    public abstract class ChangeInventoryPolicyValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IChangeInventoryPolicyCommand
    {
        protected ChangeInventoryPolicyValidator()
        {
            RuleFor(command => command.PolicyId).GreaterThan(0);
            RuleFor(command => command.ExpectedVersion).GreaterThan(0);
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
