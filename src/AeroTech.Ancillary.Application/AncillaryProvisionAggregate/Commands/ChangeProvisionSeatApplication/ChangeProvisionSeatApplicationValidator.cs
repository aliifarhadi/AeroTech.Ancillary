using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionSeatApplication
{
    public abstract class ChangeProvisionSeatApplicationValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IChangeProvisionSeatApplicationCommand
    {
        protected ChangeProvisionSeatApplicationValidator()
        {
            RuleFor(command => command.ProvisionId).GreaterThan(0);
            RuleFor(command => command.SeatApplication!)
                .SetValidator(new ProvisionSeatApplicationInputValidator())
                .When(command => command.SeatApplication is not null);
        }
    }
}
