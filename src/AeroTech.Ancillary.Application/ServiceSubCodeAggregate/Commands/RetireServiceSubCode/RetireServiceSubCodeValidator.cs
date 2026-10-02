using FluentValidation;

namespace AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Commands.RetireServiceSubCode
{
    public abstract class RetireServiceSubCodeValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IRetireServiceSubCodeCommand
    {
        protected RetireServiceSubCodeValidator()
        {
            RuleFor(command => command.ServiceSubCodeId).GreaterThan(0);
        }
    }
}
