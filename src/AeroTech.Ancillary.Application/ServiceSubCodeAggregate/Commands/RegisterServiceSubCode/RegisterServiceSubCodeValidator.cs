using FluentValidation;

namespace AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Commands.RegisterServiceSubCode
{
    public abstract class RegisterServiceSubCodeValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IRegisterServiceSubCodeCommand
    {
        protected RegisterServiceSubCodeValidator()
        {
            RuleFor(command => command.Code).NotEmpty().Matches("^[A-Z0-9]{3}$");
            RuleFor(command => command.Rfic).Matches("^[A-Z]$");
            RuleFor(command => command.GroupCode).Matches("^[A-Z0-9]{2}$");
            RuleFor(command => command.SubGroupCode).Matches("^[A-Z0-9]{2}$");
            RuleFor(command => command.Description1Code).Matches("^[A-Z0-9]{2}$");
            RuleFor(command => command.Description2Code).Matches("^[A-Z0-9]{2}$");
            RuleFor(command => command.CommercialName).Matches("^[A-Za-z0-9 ]{1,30}$");

            When(command => IsCarrierDefined(command.Code), () =>
            {
                RuleFor(command => command.Rfic).NotNull();
                RuleFor(command => command.GroupCode).NotNull();
                RuleFor(command => command.CommercialName).NotNull();
            });
        }

        private static bool IsCarrierDefined(string? code) => code is { Length: > 0 } && char.IsAsciiLetterUpper(code[0]);
    }
}
