using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryInventoryPolicyAggregate
{
    public sealed class AncillaryInventoryPolicyConfiguration : IEntityTypeConfiguration<AncillaryInventoryPolicy>
    {
        public const string CurrentIdentityIndex = "IX_AncillaryInventoryPolicies_Identity_Current";

        public void Configure(EntityTypeBuilder<AncillaryInventoryPolicy> builder)
        {
            builder.ToTable("AncillaryInventoryPolicies");
            builder.HasKey(policy => policy.Id);
            builder.Property(policy => policy.Id).ValueGeneratedNever();
            builder.Property(policy => policy.ServiceDefinitionRef).HasMaxLength(30).IsRequired();
            builder.Property(policy => policy.ProviderKey).HasMaxLength(50);
            builder.Property(policy => policy.Version).IsConcurrencyToken();
            builder.OwnsOne(policy => policy.CountConsumption, count =>
            {
                count.Property(value => value.ResourceId).HasColumnName("CountResourceId");
                count.Property(value => value.CountPerAcceptedUnit).HasColumnName("CountPerAcceptedUnit");
                count.Property(value => value.CountUnit).HasColumnName("CountUnit");
            });
            builder.OwnsOne(policy => policy.WeightConsumption, weight =>
            {
                weight.Property(value => value.WeightResourceId).HasColumnName("WeightResourceId");
                weight.Property(value => value.ConsumptionMode).HasColumnName("WeightConsumptionMode");
                weight.Property(value => value.FixedKgPerUnit).HasColumnName("WeightFixedKgPerUnit").HasPrecision(18, 3);
            });
            builder.OwnsOne(policy => policy.SlotConsumption, slot =>
            {
                slot.Property(value => value.FacilityId).HasColumnName("SlotFacilityId");
                slot.Property(value => value.OccupancyMinutes).HasColumnName("SlotOccupancyMinutes");
                slot.Property(value => value.PeoplePerAcceptedUnit).HasColumnName("SlotPeoplePerAcceptedUnit");
            });
            builder.Navigation(policy => policy.CountConsumption).IsRequired(false);
            builder.Navigation(policy => policy.WeightConsumption).IsRequired(false);
            builder.Navigation(policy => policy.SlotConsumption).IsRequired(false);
            builder.HasOne<AncillaryServiceDefinition>()
                .WithMany()
                .HasForeignKey(policy => policy.ServiceDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(policy => policy.PassengerUsageLimits)
                .WithOne()
                .HasForeignKey(limit => limit.InventoryPolicyId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(policy => policy.PassengerUsageLimits).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasIndex(policy => new { policy.OwnerAirlineId, policy.ServiceDefinitionRef }, CurrentIdentityIndex)
                .IsUnique()
                .HasFilter($"[Status] <> {(int)InventoryRecordStatus.Retired}");
            builder.HasIndex(policy => policy.Status);
        }
    }
}
