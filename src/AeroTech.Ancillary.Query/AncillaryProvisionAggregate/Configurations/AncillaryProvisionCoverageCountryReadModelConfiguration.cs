using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Configurations
{
    public sealed class AncillaryProvisionCoverageCountryReadModelConfiguration : IEntityTypeConfiguration<AncillaryProvisionCoverageCountryReadModel>
    {
        public void Configure(EntityTypeBuilder<AncillaryProvisionCoverageCountryReadModel> builder)
        {
            builder.ToTable("AncillaryProvisionCoverageCountries");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.HasIndex(row => row.AncillaryProvisionId);
        }
    }
}
