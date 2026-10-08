using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionDayTimeWindow
{
    public abstract class ChangeProvisionDayTimeWindowValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IChangeProvisionDayTimeWindowCommand
    {
        protected ChangeProvisionDayTimeWindowValidator()
        {
            RuleFor(command => command.ProvisionId).GreaterThan(0);
            RuleFor(command => command.RowId).GreaterThan(0);
            RuleFor(command => command.DaysOfWeekMask).InclusiveBetween((byte)1, (byte)127);
            RuleFor(command => command.Effect).IsInEnum();
        }
    }
}
