using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryProvisionAggregate
{
    public sealed class ProvisionFlightNumberConfiguration : IEntityTypeConfiguration<ProvisionFlightNumber>
    {
        public void Configure(EntityTypeBuilder<ProvisionFlightNumber> builder)
        {
            builder.ToTable("ProvisionFlightNumbers");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.Property(row => row.FlightNumber).HasMaxLength(16).IsRequired();
            builder.HasIndex(row => new { row.AncillaryProvisionId, row.FlightNumber }).IsUnique();
        }
    }
}
