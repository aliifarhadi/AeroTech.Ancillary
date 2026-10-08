using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Configurations
{
    public sealed class AncillaryProvisionMarketingAirlineReadModelConfiguration : IEntityTypeConfiguration<AncillaryProvisionMarketingAirlineReadModel>
    {
        public void Configure(EntityTypeBuilder<AncillaryProvisionMarketingAirlineReadModel> builder)
        {
            builder.ToTable("AncillaryProvisionMarketingAirlines");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.HasIndex(row => row.AncillaryProvisionId);
        }
    }
}
