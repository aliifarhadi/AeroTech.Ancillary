using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryProvisionAggregate
{
    public sealed class ProvisionViaAirportConfiguration : IEntityTypeConfiguration<ProvisionViaAirport>
    {
        public void Configure(EntityTypeBuilder<ProvisionViaAirport> builder)
        {
            builder.ToTable("ProvisionViaAirports");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.HasIndex(row => new { row.AncillaryProvisionId, row.AirportId }).IsUnique();
            builder.HasOne<AncillaryProvision>()
                .WithMany()
                .HasForeignKey(row => row.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
