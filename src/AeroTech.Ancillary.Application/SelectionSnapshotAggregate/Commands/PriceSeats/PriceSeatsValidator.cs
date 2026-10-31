using FluentValidation;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceSeats
{
    internal sealed class PriceSeatsValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : IPriceSeatsCommand
    {
        public PriceSeatsValidator()
        {
            RuleFor(command => command.Ids).NotEmpty();
            RuleFor(command => command.Ids.Count)
                .LessThanOrEqualTo(50)
                .When(command => command.Ids is not null);
            RuleForEach(command => command.Ids).NotEmpty().MaximumLength(256);
            RuleFor(command => command.Ids)
                .Must(ids => ids.Distinct(StringComparer.Ordinal).Count() == ids.Count)
                .When(command => command.Ids is not null)
                .WithMessage("Each id must appear once.");
        }
    }
}
