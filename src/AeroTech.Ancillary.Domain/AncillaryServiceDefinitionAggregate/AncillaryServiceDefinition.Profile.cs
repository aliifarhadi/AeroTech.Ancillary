using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Specifications;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.ValueObjects;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Messages.Ancillary.Enums;
using Routing = AeroTech.Messages.Ancillary.Enums.DocumentRouting;

namespace AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate
{
    public sealed partial class AncillaryServiceDefinition
    {
        public AncillaryProfile? Profile { get; private set; }

        public string? VariantCode { get; private set; }

        public DocumentRouting? DocumentRouting { get; private set; }

        public BaggageSpecification? Baggage { get; private set; }

        public SeatSpecification? Seat { get; private set; }

        public UpgradeSpecification? Upgrade { get; private set; }

        public MealSpecification? Meal { get; private set; }

        public PetSpecification? Pet { get; private set; }

        public AssistedTravelSpecification? AssistedTravel { get; private set; }

        public AirportServiceSpecification? AirportService { get; private set; }

        public PrioritySpecification? Priority { get; private set; }

        public ConnectivitySpecification? Connectivity { get; private set; }

        public bool IsClassified => Profile is not null && Variant is not null;

        public AncillaryVariant? Variant => AncillaryVariant.Find(VariantCode);

        public CustomerSelectionContract? SelectionContract => IsClassified ? CustomerSelectionContract.For(this) : null;

        public ServiceSpecificationArgs SpecificationArgs()
            => new(
                Baggage?.ToArgs(),
                Seat?.ToArgs(),
                Upgrade?.ToArgs(),
                Meal?.ToArgs(),
                Pet?.ToArgs(),
                AssistedTravel?.ToArgs(),
                AirportService?.ToArgs(),
                Priority?.ToArgs(),
                Connectivity?.ToArgs());

        public IReadOnlyCollection<int> ReferencedAirportIds()
        {
            var airports = new List<int>();

            if (AirportService is not null)
                airports.Add(AirportService.AirportId);

            if (Priority is not null)
                airports.AddRange(Priority.Airports.Select(row => (int)row.ReferenceId));

            if (AssistedTravel?.UnaccompaniedMinor is { } minor)
                airports.AddRange(minor.AllowedTransitAirports.Select(row => (int)row.ReferenceId));

            return airports.Distinct().ToList();
        }

        private void EnsureClassified()
        {
            if (!IsClassified)
                throw ExceptionFactory.ServiceDefinitionNotClassified();
        }

        private void ApplyProfile(
            ServiceDefinitionProfileArgs profile,
            PricingUnit pricingUnit,
            ServiceDateBasis serviceDateBasis,
            DocumentDefinition document,
            BookingDefinition booking)
        {
            Require(profile is not null && Enum.IsDefined(profile.Profile), nameof(Profile));

            var variant = AncillaryVariant.Find(profile!.VariantCode);

            Require(variant is not null && variant.Profile == profile.Profile, nameof(VariantCode));

            if (Version > 1 && Profile is not null && (Profile != profile.Profile || VariantCode != variant!.Code))
                throw ExceptionFactory.ServiceDefinitionProfileIsImmutable();

            Require(variant!.PricingUnits.Contains(pricingUnit), nameof(PricingUnit));
            Require(variant.ServiceDateBases.Contains(serviceDateBasis), nameof(ServiceDateBasis));
            Require(Enum.IsDefined(profile.DocumentRouting), nameof(DocumentRouting));
            Require(
                profile.DocumentRouting == Routing.Emd
                    ? document.Type != AncillaryDocumentType.None
                    : document.Type == AncillaryDocumentType.None,
                nameof(DocumentRouting));

            var supplied = profile.Specification ?? new ServiceSpecificationArgs();
            var members = new (AncillaryProfile Profile, object? Value)[]
            {
                (AncillaryProfile.Baggage, supplied.Baggage),
                (AncillaryProfile.Seat, supplied.Seat),
                (AncillaryProfile.Upgrade, supplied.Upgrade),
                (AncillaryProfile.Meal, supplied.Meal),
                (AncillaryProfile.Pet, supplied.Pet),
                (AncillaryProfile.AssistedTravel, supplied.AssistedTravel),
                (AncillaryProfile.AirportService, supplied.AirportService),
                (AncillaryProfile.Priority, supplied.Priority),
                (AncillaryProfile.Connectivity, supplied.Connectivity)
            };

            if (members.Count(member => member.Value is not null) != 1 || members.Single(member => member.Value is not null).Profile != profile.Profile)
                throw ExceptionFactory.ServiceDefinitionSpecificationMismatch(profile.Profile, variant.Code);

            var baggage = supplied.Baggage is null ? null : BaggageSpecification.Create(variant, supplied.Baggage);
            var seat = supplied.Seat is null ? null : SeatSpecification.Create(variant, supplied.Seat);
            var upgrade = supplied.Upgrade is null ? null : UpgradeSpecification.Create(supplied.Upgrade);
            var meal = supplied.Meal is null ? null : MealSpecification.Create(variant, supplied.Meal);
            var pet = supplied.Pet is null ? null : PetSpecification.Create(variant, supplied.Pet);
            var assistedTravel = supplied.AssistedTravel is null ? null : AssistedTravelSpecification.Create(variant, supplied.AssistedTravel);
            var airportService = supplied.AirportService is null ? null : AirportServiceSpecification.Create(variant, supplied.AirportService);
            var priority = supplied.Priority is null ? null : PrioritySpecification.Create(supplied.Priority);
            var connectivity = supplied.Connectivity is null ? null : ConnectivitySpecification.Create(supplied.Connectivity);
            var externalTicket = seat?.RequiresExternalTicketAction == true || upgrade?.RequiresTicketExchange == true;

            Require(externalTicket == (profile.DocumentRouting == Routing.TicketOrExchange), nameof(DocumentRouting));
            Require(pet is null || booking.ConfirmationRequirement == ConfirmationRequirement.SubjectToConfirmation, nameof(Booking));
            Require(
                meal is not { MealKind: MealKind.SpecialRequest } || (booking.Method == BookingMethod.Ssr && booking.SsrCode == meal.MealCode),
                nameof(Booking));
            Require(
                assistedTravel?.Wheelchair is null || IsSsrOf(booking, assistedTravel.Wheelchair.AllowedSsrCodes),
                nameof(Booking));
            Require(
                assistedTravel?.DisabilityAssistance is null || IsSsrOf(booking, assistedTravel.DisabilityAssistance.AllowedSsrCodes),
                nameof(Booking));
            Require(
                assistedTravel?.MedicalEquipment is not { } medical
                || (booking.Method == BookingMethod.Ssr
                    && booking.SsrCode == medical.MedicalServiceCode
                    && (!medical.RequiresMedicalApproval || booking.ConfirmationRequirement == ConfirmationRequirement.SubjectToConfirmation)),
                nameof(Booking));

            Profile = profile.Profile;
            VariantCode = variant.Code;
            DocumentRouting = profile.DocumentRouting;
            Baggage = baggage;
            Seat = seat;
            Upgrade = upgrade;
            Meal = meal;
            Pet = pet;
            AssistedTravel = assistedTravel;
            AirportService = airportService;
            Priority = priority;
            Connectivity = connectivity;
        }

        private void CopyProfileTo(AncillaryServiceDefinition revision)
        {
            if (Variant is not { } variant)
                return;

            revision.Profile = Profile;
            revision.VariantCode = VariantCode;
            revision.DocumentRouting = DocumentRouting;
            revision.Baggage = Baggage is null ? null : BaggageSpecification.Create(variant, Baggage.ToArgs());
            revision.Seat = Seat is null ? null : SeatSpecification.Create(variant, Seat.ToArgs());
            revision.Upgrade = Upgrade is null ? null : UpgradeSpecification.Create(Upgrade.ToArgs());
            revision.Meal = Meal is null ? null : MealSpecification.Create(variant, Meal.ToArgs());
            revision.Pet = Pet is null ? null : PetSpecification.Create(variant, Pet.ToArgs());
            revision.AssistedTravel = AssistedTravel is null ? null : AssistedTravelSpecification.Create(variant, AssistedTravel.ToArgs());
            revision.AirportService = AirportService is null ? null : AirportServiceSpecification.Create(variant, AirportService.ToArgs());
            revision.Priority = Priority is null ? null : PrioritySpecification.Create(Priority.ToArgs());
            revision.Connectivity = Connectivity is null ? null : ConnectivitySpecification.Create(Connectivity.ToArgs());
        }

        private static bool IsSsrOf(BookingDefinition booking, IEnumerable<SpecificationCode> allowed)
            => booking.Method == BookingMethod.Ssr && allowed.Any(row => row.Code == booking.SsrCode);
    }
}
