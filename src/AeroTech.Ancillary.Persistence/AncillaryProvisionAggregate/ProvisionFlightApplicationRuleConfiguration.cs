using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryProvisionAggregate
{
    public sealed class ProvisionFlightApplicationRuleConfiguration : IEntityTypeConfiguration<ProvisionFlightApplicationRule>
    {
        public void Configure(EntityTypeBuilder<ProvisionFlightApplicationRule> builder)
        {
            builder.ToTable("ProvisionFlightApplicationRules");
            builder.HasKey(rule => rule.Id);
            builder.Property(rule => rule.Id).ValueGeneratedNever();
            builder.HasMany(rule => rule.MarketingAirlines)
                .WithOne()
                .HasForeignKey(row => row.ProvisionFlightApplicationRuleId)
                .OnDelete(DeleteBehavior.ClientCascade);
            builder.Navigation(rule => rule.MarketingAirlines).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasMany(rule => rule.OperatingAirlines)
                .WithOne()
                .HasForeignKey(row => row.ProvisionFlightApplicationRuleId)
                .OnDelete(DeleteBehavior.ClientCascade);
            builder.Navigation(rule => rule.OperatingAirlines).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasMany(rule => rule.FlightNumbers)
                .WithOne()
                .HasForeignKey(row => row.ProvisionFlightApplicationRuleId)
                .OnDelete(DeleteBehavior.ClientCascade);
            builder.Navigation(rule => rule.FlightNumbers).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasMany(rule => rule.Flights)
                .WithOne()
                .HasForeignKey(row => row.ProvisionFlightApplicationRuleId)
                .OnDelete(DeleteBehavior.ClientCascade);
            builder.Navigation(rule => rule.Flights).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasMany(rule => rule.Aircraft)
                .WithOne()
                .HasForeignKey(row => row.ProvisionFlightApplicationRuleId)
                .OnDelete(DeleteBehavior.ClientCascade);
            builder.Navigation(rule => rule.Aircraft).UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
