using FluentValidation;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices
{
    public sealed class HoldCoverageInputValidator : AbstractValidator<HoldCoverageInput>
    {
        public HoldCoverageInputValidator()
        {
            RuleFor(coverage => coverage.FlightIds).NotEmpty();
            RuleForEach(coverage => coverage.FlightIds).GreaterThan(0);
            RuleFor(coverage => coverage.FlightIds)
                .Must(flightIds => flightIds.Distinct().Count() == flightIds.Count)
                .When(coverage => coverage.FlightIds is not null)
                .WithMessage("Each flightId must appear once.");
        }
    }
}
