using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionDayTimeWindow
{
    public abstract class AddProvisionDayTimeWindowValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IAddProvisionDayTimeWindowCommand
    {
        protected AddProvisionDayTimeWindowValidator()
        {
            RuleFor(command => command.ProvisionId).GreaterThan(0);
            RuleFor(command => command.DaysOfWeekMask).InclusiveBetween((byte)1, (byte)127);
            RuleFor(command => command.Effect).IsInEnum();
        }
    }
}
