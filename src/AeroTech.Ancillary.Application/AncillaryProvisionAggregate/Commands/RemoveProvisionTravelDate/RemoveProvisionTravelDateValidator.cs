using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionTravelDate
{
    public abstract class RemoveProvisionTravelDateValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IRemoveProvisionTravelDateCommand
    {
        protected RemoveProvisionTravelDateValidator()
        {
            RuleFor(command => command.ProvisionId).GreaterThan(0);
            RuleFor(command => command.RowId).GreaterThan(0);
        }
    }
}
