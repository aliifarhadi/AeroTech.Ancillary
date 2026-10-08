using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryProvisionAggregate
{
    public sealed class ProvisionSeatApplicationRuleConfiguration : IEntityTypeConfiguration<ProvisionSeatApplicationRule>
    {
        public void Configure(EntityTypeBuilder<ProvisionSeatApplicationRule> builder)
        {
            builder.ToTable("ProvisionSeatApplicationRules");
            builder.HasKey(rule => rule.Id);
            builder.Property(rule => rule.Id).ValueGeneratedNever();
            builder.HasMany(rule => rule.SeatNumbers)
                .WithOne()
                .HasForeignKey(row => row.ProvisionSeatApplicationRuleId)
                .OnDelete(DeleteBehavior.ClientCascade);
            builder.Navigation(rule => rule.SeatNumbers).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasMany(rule => rule.SeatCharacteristics)
                .WithOne()
                .HasForeignKey(row => row.ProvisionSeatApplicationRuleId)
                .OnDelete(DeleteBehavior.ClientCascade);
            builder.Navigation(rule => rule.SeatCharacteristics).UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
