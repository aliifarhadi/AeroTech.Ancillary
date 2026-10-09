using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryProvisionAggregate
{
    public sealed class ProvisionAirportServiceRuleConfiguration : IEntityTypeConfiguration<ProvisionAirportServiceRule>
    {
        public void Configure(EntityTypeBuilder<ProvisionAirportServiceRule> builder)
        {
            builder.ToTable("ProvisionAirportServiceRules");
            builder.HasKey(rule => rule.Id);
            builder.Property(rule => rule.Id).ValueGeneratedNever();
            builder.Property(rule => rule.TerminalRef).HasMaxLength(30);
        }
    }
}
