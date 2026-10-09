using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryProvisionAggregate
{
    public sealed class ProvisionPetRuleConfiguration : IEntityTypeConfiguration<ProvisionPetRule>
    {
        public void Configure(EntityTypeBuilder<ProvisionPetRule> builder)
        {
            builder.ToTable("ProvisionPetRules");
            builder.HasKey(rule => rule.Id);
            builder.Property(rule => rule.Id).ValueGeneratedNever();
            builder.Property(rule => rule.CountryExceptionCode).HasMaxLength(10);
            builder.Property(rule => rule.MaxCombinedKgOverride).HasPrecision(18, 3);
        }
    }
}
