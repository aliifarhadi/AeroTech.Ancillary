using FluentValidation;

namespace AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Commands.ReactivateServiceSubCode
{
    public abstract class ReactivateServiceSubCodeValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IReactivateServiceSubCodeCommand
    {
        protected ReactivateServiceSubCodeValidator()
        {
            RuleFor(command => command.ServiceSubCodeId).GreaterThan(0);
        }
    }
}
