using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryProvisionAggregate
{
    public sealed class ProvisionPassengerEligibilityRuleConfiguration : IEntityTypeConfiguration<ProvisionPassengerEligibilityRule>
    {
        public void Configure(EntityTypeBuilder<ProvisionPassengerEligibilityRule> builder)
        {
            builder.ToTable("ProvisionPassengerEligibilityRules");
            builder.HasKey(rule => rule.Id);
            builder.Property(rule => rule.Id).ValueGeneratedNever();
            builder.HasMany(rule => rule.PassengerTypes)
                .WithOne()
                .HasForeignKey(row => row.ProvisionPassengerEligibilityRuleId)
                .OnDelete(DeleteBehavior.ClientCascade);
            builder.Navigation(rule => rule.PassengerTypes).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasMany(rule => rule.AgeBands)
                .WithOne()
                .HasForeignKey(row => row.ProvisionPassengerEligibilityRuleId)
                .OnDelete(DeleteBehavior.ClientCascade);
            builder.Navigation(rule => rule.AgeBands).UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
