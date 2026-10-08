using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionDayTimeApplication
{
    public abstract class ChangeProvisionDayTimeApplicationValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IChangeProvisionDayTimeApplicationCommand
    {
        protected ChangeProvisionDayTimeApplicationValidator()
        {
            RuleFor(command => command.ProvisionId).GreaterThan(0);
            RuleFor(command => command.DayTimeApplication!)
                .SetValidator(new ProvisionDayTimeApplicationInputValidator())
                .When(command => command.DayTimeApplication is not null);
        }
    }
}
