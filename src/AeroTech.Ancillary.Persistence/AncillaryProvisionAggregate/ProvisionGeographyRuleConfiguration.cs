using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryProvisionAggregate
{
    public sealed class ProvisionGeographyRuleConfiguration : IEntityTypeConfiguration<ProvisionGeographyRule>
    {
        public void Configure(EntityTypeBuilder<ProvisionGeographyRule> builder)
        {
            builder.ToTable("ProvisionGeographyRules");
            builder.HasKey(rule => rule.Id);
            builder.Property(rule => rule.Id).ValueGeneratedNever();
            builder.HasMany(rule => rule.OriginAirports)
                .WithOne()
                .HasForeignKey(row => row.ProvisionGeographyRuleId)
                .OnDelete(DeleteBehavior.ClientCascade);
            builder.Navigation(rule => rule.OriginAirports).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasMany(rule => rule.DestinationAirports)
                .WithOne()
                .HasForeignKey(row => row.ProvisionGeographyRuleId)
                .OnDelete(DeleteBehavior.ClientCascade);
            builder.Navigation(rule => rule.DestinationAirports).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasMany(rule => rule.ViaAirports)
                .WithOne()
                .HasForeignKey(row => row.ProvisionGeographyRuleId)
                .OnDelete(DeleteBehavior.ClientCascade);
            builder.Navigation(rule => rule.ViaAirports).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasMany(rule => rule.CoverageCountries)
                .WithOne()
                .HasForeignKey(row => row.ProvisionGeographyRuleId)
                .OnDelete(DeleteBehavior.ClientCascade);
            builder.Navigation(rule => rule.CoverageCountries).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasMany(rule => rule.RoutePairs)
                .WithOne()
                .HasForeignKey(row => row.ProvisionGeographyRuleId)
                .OnDelete(DeleteBehavior.ClientCascade);
            builder.Navigation(rule => rule.RoutePairs).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasMany(rule => rule.ServiceLocations)
                .WithOne()
                .HasForeignKey(row => row.ProvisionGeographyRuleId)
                .OnDelete(DeleteBehavior.ClientCascade);
            builder.Navigation(rule => rule.ServiceLocations).UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
