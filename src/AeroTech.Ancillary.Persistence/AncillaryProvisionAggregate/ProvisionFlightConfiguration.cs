using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryProvisionAggregate
{
    public sealed class ProvisionFlightConfiguration : IEntityTypeConfiguration<ProvisionFlight>
    {
        public void Configure(EntityTypeBuilder<ProvisionFlight> builder)
        {
            builder.ToTable("ProvisionFlights");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.HasIndex(row => new { row.AncillaryProvisionId, row.FlightId }).IsUnique();
            builder.HasOne<AncillaryProvision>()
                .WithMany()
                .HasForeignKey(row => row.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
