using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryProvisionAggregate
{
    public sealed class ProvisionMarketingAirlineConfiguration : IEntityTypeConfiguration<ProvisionMarketingAirline>
    {
        public void Configure(EntityTypeBuilder<ProvisionMarketingAirline> builder)
        {
            builder.ToTable("ProvisionMarketingAirlines");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.HasIndex(row => new { row.AncillaryProvisionId, row.AirlineId }).IsUnique();
        }
    }
}
