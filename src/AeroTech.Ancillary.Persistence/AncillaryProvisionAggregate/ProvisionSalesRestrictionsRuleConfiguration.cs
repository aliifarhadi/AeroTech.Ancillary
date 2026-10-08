using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryProvisionAggregate
{
    public sealed class ProvisionSalesRestrictionsRuleConfiguration : IEntityTypeConfiguration<ProvisionSalesRestrictionsRule>
    {
        public void Configure(EntityTypeBuilder<ProvisionSalesRestrictionsRule> builder)
        {
            builder.ToTable("ProvisionSalesRestrictionsRules");
            builder.HasKey(rule => rule.Id);
            builder.Property(rule => rule.Id).ValueGeneratedNever();
            builder.HasMany(rule => rule.PointsOfSale)
                .WithOne()
                .HasForeignKey(row => row.ProvisionSalesRestrictionsRuleId)
                .OnDelete(DeleteBehavior.ClientCascade);
            builder.Navigation(rule => rule.PointsOfSale).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasMany(rule => rule.Customers)
                .WithOne()
                .HasForeignKey(row => row.ProvisionSalesRestrictionsRuleId)
                .OnDelete(DeleteBehavior.ClientCascade);
            builder.Navigation(rule => rule.Customers).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasMany(rule => rule.CustomerTypes)
                .WithOne()
                .HasForeignKey(row => row.ProvisionSalesRestrictionsRuleId)
                .OnDelete(DeleteBehavior.ClientCascade);
            builder.Navigation(rule => rule.CustomerTypes).UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
