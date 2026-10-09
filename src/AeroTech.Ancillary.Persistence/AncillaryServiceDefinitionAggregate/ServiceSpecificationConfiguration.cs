using System.Linq.Expressions;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Specifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryServiceDefinitionAggregate
{
    internal static class ServiceSpecificationConfiguration
    {
        private const string DefinitionKey = "AncillaryServiceDefinitionId";

        public static void OwnsSpecifications(this EntityTypeBuilder<AncillaryServiceDefinition> builder)
        {
            builder.Property(definition => definition.VariantCode).HasMaxLength(8);
            builder.Ignore(definition => definition.IsClassified);
            builder.Ignore(definition => definition.Variant);
            builder.Ignore(definition => definition.SelectionContract);

            builder.OwnsOne(definition => definition.Baggage, specification =>
            {
                specification.OwnedBy("BaggageSpecifications");
                specification.Property(value => value.PackageWeightKg).HasPrecision(18, 3);
                specification.Property(value => value.MaxKgPerPiece).HasPrecision(18, 3);
                specification.Property(value => value.WeightFromExclusiveKg).HasPrecision(18, 3);
                specification.Property(value => value.WeightToInclusiveKg).HasPrecision(18, 3);
                specification.Property(value => value.MaxLinearSumCm).HasPrecision(18, 3);
                specification.Property(value => value.EquipmentKind).HasMaxLength(20);
                specification.OwnsOne(value => value.MaxSize, size => size.Dimensions("Max"));
            });

            builder.OwnsOne(definition => definition.Seat, specification =>
            {
                specification.OwnedBy("SeatSpecifications");
                specification.Codes(value => value.SeatCharacteristicCodes, "SeatSpecificationCharacteristicCodes", 25);
                specification.References(value => value.ApplicableCabins, "SeatSpecificationCabins", "CabinClassId");
            });

            builder.OwnsOne(definition => definition.Upgrade, specification =>
            {
                specification.OwnedBy("UpgradeSpecifications");
                specification.References(value => value.EligibleFareFamilies, "UpgradeSpecificationFareFamilies", "FareFamilyId");
            });

            builder.OwnsOne(definition => definition.Meal, specification =>
            {
                specification.OwnedBy("MealSpecifications");
                specification.Property(value => value.MealCode).HasMaxLength(4);
                specification.Property(value => value.MenuItemRef).HasMaxLength(40);
                specification.Property(value => value.DietaryCode).HasMaxLength(20);
                specification.Property(value => value.ExclusiveMealFamilyCode).HasMaxLength(30);
            });

            builder.OwnsOne(definition => definition.Pet, specification =>
            {
                specification.OwnedBy("PetSpecifications");
                specification.Property(value => value.MaxCombinedWeightKg).HasPrecision(18, 3);
                specification.OwnsOne(value => value.CarrierDimensionsMaxCm, size => size.Dimensions("CarrierMax"));
                specification.Navigation(value => value.CarrierDimensionsMaxCm).IsRequired();
                specification.Codes(value => value.RequiredDocumentCodes, "PetSpecificationDocumentCodes", 30);
                specification.OwnsMany(value => value.AllowedAnimalTypes, rows =>
                {
                    rows.ToTable("PetSpecificationAnimalTypes");
                    rows.WithOwner().HasForeignKey(DefinitionKey);
                    rows.Property(row => row.OtherCode).HasMaxLength(10);
                    rows.HasKey(DefinitionKey, nameof(PetAnimal.AnimalType), nameof(PetAnimal.OtherCode));
                });
                specification.Navigation(value => value.AllowedAnimalTypes).UsePropertyAccessMode(PropertyAccessMode.Field);
                specification.OwnsMany(value => value.AllowedHoldAnimalSizeBrackets, rows =>
                {
                    rows.ToTable("PetSpecificationSizeBrackets");
                    rows.WithOwner().HasForeignKey(DefinitionKey);
                    rows.Property(row => row.Code).HasMaxLength(20);
                    rows.Property(row => row.WeightFromExclusiveKg).HasPrecision(18, 3);
                    rows.Property(row => row.WeightToInclusiveKg).HasPrecision(18, 3);
                    rows.HasKey(DefinitionKey, nameof(PetSizeBracket.Code));
                });
                specification.Navigation(value => value.AllowedHoldAnimalSizeBrackets).UsePropertyAccessMode(PropertyAccessMode.Field);
            });

            builder.OwnsOne(definition => definition.AssistedTravel, specification =>
            {
                specification.OwnedBy("AssistedTravelSpecifications");
                specification.OwnsOne(value => value.Wheelchair, details =>
                {
                    details.OwnedBy("AssistedTravelWheelchairDetails");
                    details.Property(value => value.AssistanceLevelCode).HasMaxLength(20);
                    details.Codes(value => value.AllowedSsrCodes, "AssistedTravelWheelchairSsrCodes", 4);
                });
                specification.OwnsOne(value => value.DisabilityAssistance, details =>
                {
                    details.OwnedBy("AssistedTravelDisabilityDetails");
                    details.Codes(value => value.AllowedSsrCodes, "AssistedTravelDisabilitySsrCodes", 4);
                });
                specification.OwnsOne(value => value.MedicalEquipment, details =>
                {
                    details.OwnedBy("AssistedTravelMedicalDetails");
                    details.Property(value => value.MedicalServiceCode).HasMaxLength(4);
                    details.Property(value => value.OxygenUnits).HasPrecision(18, 3);
                    details.Codes(value => value.EvidenceTypeCodes, "AssistedTravelMedicalEvidenceCodes", 30);
                });
                specification.OwnsOne(value => value.Bassinet, details =>
                {
                    details.OwnedBy("AssistedTravelBassinetDetails");
                    details.Property(value => value.MaxInfantWeightKg).HasPrecision(18, 3);
                    details.Codes(value => value.CompatibleSeatGroups, "AssistedTravelBassinetSeatGroups", 25);
                });
                specification.OwnsOne(value => value.UnaccompaniedMinor, details =>
                {
                    details.OwnedBy("AssistedTravelMinorDetails");
                    details.References(value => value.AllowedTransitAirports, "AssistedTravelMinorTransitAirports", "AirportId");
                });
            });

            builder.OwnsOne(definition => definition.AirportService, specification =>
            {
                specification.OwnedBy("AirportServiceSpecifications");
                specification.Property(value => value.TerminalRef).HasMaxLength(30);
                specification.Property(value => value.IanaTimeZone).HasMaxLength(64);
                specification.Codes(value => value.IncludedComponentCodes, "AirportServiceSpecificationComponents", 30);
            });

            builder.OwnsOne(definition => definition.Priority, specification =>
            {
                specification.OwnedBy("PrioritySpecifications");
                specification.Property(value => value.PriorityZoneCode).HasMaxLength(20);
                specification.Property(value => value.PriorityGroupCode).HasMaxLength(20);
                specification.Property(value => value.FareBenefitRef).HasMaxLength(40);
                specification.References(value => value.Airports, "PrioritySpecificationAirports", "AirportId");
            });

            builder.OwnsOne(definition => definition.Connectivity, specification =>
            {
                specification.OwnedBy("ConnectivitySpecifications");
                specification.Property(value => value.FulfillmentProviderRef).HasMaxLength(50);
                specification.References(value => value.EligibleAircraft, "ConnectivitySpecificationAircraft", "AircraftId");
            });
        }

        private static void OwnedBy<TOwner, TOwned>(this OwnedNavigationBuilder<TOwner, TOwned> owned, string table)
            where TOwner : class
            where TOwned : class
        {
            owned.ToTable(table);
            owned.WithOwner().HasForeignKey(DefinitionKey);
        }

        private static void Dimensions<TOwner>(this OwnedNavigationBuilder<TOwner, DimensionsCm> size, string prefix)
            where TOwner : class
        {
            size.Property(value => value.LengthCm).HasColumnName($"{prefix}LengthCm").HasPrecision(18, 3);
            size.Property(value => value.WidthCm).HasColumnName($"{prefix}WidthCm").HasPrecision(18, 3);
            size.Property(value => value.HeightCm).HasColumnName($"{prefix}HeightCm").HasPrecision(18, 3);
        }

        private static void Codes<TOwner, TOwned>(
            this OwnedNavigationBuilder<TOwner, TOwned> owned,
            Expression<Func<TOwned, IEnumerable<SpecificationCode>?>> navigation,
            string table,
            int maxLength)
            where TOwner : class
            where TOwned : class
        {
            owned.OwnsMany(navigation, rows =>
            {
                rows.ToTable(table);
                rows.WithOwner().HasForeignKey(DefinitionKey);
                rows.Property(row => row.Code).HasMaxLength(maxLength);
                rows.HasKey(DefinitionKey, nameof(SpecificationCode.Code));
            });
            owned.Navigation(navigation).UsePropertyAccessMode(PropertyAccessMode.Field);
        }

        private static void References<TOwner, TOwned>(
            this OwnedNavigationBuilder<TOwner, TOwned> owned,
            Expression<Func<TOwned, IEnumerable<SpecificationReference>?>> navigation,
            string table,
            string column)
            where TOwner : class
            where TOwned : class
        {
            owned.OwnsMany(navigation, rows =>
            {
                rows.ToTable(table);
                rows.WithOwner().HasForeignKey(DefinitionKey);
                rows.Property(row => row.ReferenceId).HasColumnName(column).ValueGeneratedNever();
                rows.HasKey(DefinitionKey, nameof(SpecificationReference.ReferenceId));
            });
            owned.Navigation(navigation).UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
