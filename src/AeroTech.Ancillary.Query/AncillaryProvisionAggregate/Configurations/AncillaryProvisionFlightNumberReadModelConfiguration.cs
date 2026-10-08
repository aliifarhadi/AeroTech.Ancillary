using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Configurations
{
    public sealed class AncillaryProvisionFlightNumberReadModelConfiguration : IEntityTypeConfiguration<AncillaryProvisionFlightNumberReadModel>
    {
        public void Configure(EntityTypeBuilder<AncillaryProvisionFlightNumberReadModel> builder)
        {
            builder.ToTable("AncillaryProvisionFlightNumbers");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.Property(row => row.FlightNumber).HasMaxLength(16).IsRequired();
            builder.HasIndex(row => row.AncillaryProvisionId);
        }
    }
}
