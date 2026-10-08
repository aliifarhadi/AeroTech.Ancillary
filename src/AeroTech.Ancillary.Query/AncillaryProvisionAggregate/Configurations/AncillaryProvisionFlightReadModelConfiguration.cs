using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Configurations
{
    public sealed class AncillaryProvisionFlightReadModelConfiguration : IEntityTypeConfiguration<AncillaryProvisionFlightReadModel>
    {
        public void Configure(EntityTypeBuilder<AncillaryProvisionFlightReadModel> builder)
        {
            builder.ToTable("AncillaryProvisionFlights");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.HasIndex(row => row.AncillaryProvisionId);
        }
    }
}
