using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate
{
    public sealed record CustomerSelectionField(string Name, SelectionFieldType Type, bool Required);

    public sealed record CustomerSelectionContract(
        SelectionKind Kind,
        bool ZeroQuantityMeansNoSelection,
        IReadOnlyList<CustomerSelectionField> Fields)
    {
        private const string TravellerRef = "TravellerRef";
        private const string BoundRef = "BoundRef";
        private const string FlightRef = "FlightRef";
        private const string Quantity = "Quantity";

        internal static CustomerSelectionContract For(AncillaryServiceDefinition definition)
        {
            var fields = new List<CustomerSelectionField>();

            void Add(string name, SelectionFieldType type, bool required = true) => fields.Add(new CustomerSelectionField(name, type, required));

            CustomerSelectionContract Build(SelectionKind kind, params string[] references)
            {
                foreach (var reference in references)
                    Add(reference, SelectionFieldType.Reference);

                return new CustomerSelectionContract(kind, kind == SelectionKind.QuantityChoice, fields);
            }

            switch (definition.VariantCode)
            {
                case AncillaryVariant.ExtraCheckedBag or AncillaryVariant.CabinBag:
                    Add(Quantity, SelectionFieldType.Integer);

                    return Build(SelectionKind.QuantityChoice, TravellerRef, BoundRef);
                case AncillaryVariant.ExtraWeightPackage:
                    Add("PackageProductRef", SelectionFieldType.Code);
                    Add(Quantity, SelectionFieldType.Integer);

                    return Build(SelectionKind.QuantityChoice, TravellerRef, BoundRef);
                case AncillaryVariant.Overweight:
                    Add("BagRef", SelectionFieldType.Reference);
                    Add("MeasuredWeightKg", SelectionFieldType.Decimal);

                    return Build(SelectionKind.TypedForm, TravellerRef, BoundRef);
                case AncillaryVariant.Oversize:
                    Add("Dimensions", SelectionFieldType.DimensionsCm);

                    return Build(SelectionKind.TypedForm, TravellerRef, BoundRef);
                case AncillaryVariant.SpecialEquipment:
                    Add("EquipmentKind", SelectionFieldType.Code);
                    Add("Dimensions", SelectionFieldType.DimensionsCm, definition.Baggage!.MaxSize is not null || definition.Baggage.MaxLinearSumCm is not null);
                    Add("WeightKg", SelectionFieldType.Decimal, definition.Baggage.MaxKgPerPiece is not null);

                    return Build(SelectionKind.TypedForm, TravellerRef, BoundRef);
                case AncillaryVariant.StandardSeat or AncillaryVariant.PreferredSeat or AncillaryVariant.ExtraSeat:
                    Add("SeatNumber", SelectionFieldType.Text);

                    if (definition.Seat!.RequiresExitRowEligibility)
                        Add("ExitRowTermsAccepted", SelectionFieldType.Boolean);

                    if (definition.Seat.SeatPurpose == SeatPurpose.ExtraSeat)
                        Add("Purpose", SelectionFieldType.Code);

                    return Build(SelectionKind.SeatMapSelection, TravellerRef, FlightRef);
                case AncillaryVariant.CabinUpgrade:
                    Add("TargetCabinId", SelectionFieldType.Reference);
                    Add("QuoteRef", SelectionFieldType.Reference, definition.Upgrade!.AllowedUpgradeKind != UpgradeKind.FixedAncillary);

                    return Build(SelectionKind.ExternalQuote, TravellerRef, FlightRef);
                case AncillaryVariant.FreeSpecialMeal:
                    Add("MealCode", SelectionFieldType.Code);

                    return Build(SelectionKind.TypedForm, TravellerRef, FlightRef);
                case AncillaryVariant.PaidPreorderMeal:
                    Add("MenuItemRef", SelectionFieldType.Code);
                    Add(Quantity, SelectionFieldType.Integer);

                    return Build(SelectionKind.QuantityChoice, TravellerRef, FlightRef);
                case AncillaryVariant.PetInCabin or AncillaryVariant.PetInHold:
                    Add("AnimalType", SelectionFieldType.Code);
                    Add("CombinedWeightKg", SelectionFieldType.Decimal);
                    Add("CarrierDimensions", SelectionFieldType.DimensionsCm);

                    if (definition.Pet!.RequiredDocumentCodes.Count > 0)
                        Add("DocumentAcknowledgements", SelectionFieldType.CodeSet);

                    if (definition.Pet.AllowedHoldAnimalSizeBrackets.Count > 0)
                        Add("SizeBracket", SelectionFieldType.Code);

                    return Build(SelectionKind.TypedForm, TravellerRef, BoundRef);
                case AncillaryVariant.Wheelchair or AncillaryVariant.DisabilityAssistance:
                    Add("AssistanceSsrCode", SelectionFieldType.Code);

                    return Build(SelectionKind.TypedForm, TravellerRef, FlightRef);
                case AncillaryVariant.MedicalEquipment:
                    Add("EquipmentCode", SelectionFieldType.Code);

                    if (definition.AssistedTravel!.MedicalEquipment!.EvidenceTypeCodes.Count > 0)
                        Add("EvidenceDocumentRefs", SelectionFieldType.CodeSet);

                    if (definition.AssistedTravel.MedicalEquipment.EquipmentKind == MedicalEquipmentKind.Oxygen)
                        Add("OxygenUnits", SelectionFieldType.Decimal);

                    return Build(SelectionKind.TypedForm, TravellerRef, FlightRef);
                case AncillaryVariant.Bassinet:
                    return Build(SelectionKind.TypedForm, "InfantRef", "GuardianRef", FlightRef);
                case AncillaryVariant.UnaccompaniedMinor:
                    Add("GuardianHandoffContact", SelectionFieldType.Text);
                    Add("GuardianPickupContact", SelectionFieldType.Text);

                    return Build(SelectionKind.TypedForm, "ChildRef", BoundRef);
                case AncillaryVariant.Lounge or AncillaryVariant.FastTrack or AncillaryVariant.CipMeetAssist:
                    Add("AirportId", SelectionFieldType.Reference);
                    Add("FacilityRef", SelectionFieldType.Reference, definition.AirportService!.FacilityId is not null);
                    Add("TimeWithOffset", SelectionFieldType.DateTimeWithOffset, definition.AirportService.RequiresSpecificAppointment);

                    if (definition.AirportService.MaxGuestsPerPrimary is > 0)
                        Add("GuestCount", SelectionFieldType.Integer, false);

                    return Build(SelectionKind.TypedForm, TravellerRef);
                case AncillaryVariant.PriorityBoardingCheckin:
                    Add("OptIn", SelectionFieldType.Boolean);

                    return Build(SelectionKind.SimpleOptIn, TravellerRef, FlightRef);
                default:
                    Add("PlanCode", SelectionFieldType.Code);

                    if (definition.Connectivity!.MaxDevices is not null)
                        Add("DeviceCount", SelectionFieldType.Integer, false);

                    return Build(SelectionKind.TypedForm, TravellerRef, FlightRef);
            }
        }
    }
}
