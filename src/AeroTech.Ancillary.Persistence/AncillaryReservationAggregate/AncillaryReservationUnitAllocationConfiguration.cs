using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryReservationAggregate
{
    public sealed class AncillaryReservationUnitAllocationConfiguration : IEntityTypeConfiguration<AncillaryReservationUnitAllocation>
    {
        public void Configure(EntityTypeBuilder<AncillaryReservationUnitAllocation> builder)
        {
            builder.ToTable("AncillaryReservationUnitAllocations");
            builder.HasKey(allocation => allocation.Id);
            builder.Property(allocation => allocation.Id).ValueGeneratedNever();

            builder.Property(allocation => allocation.Amount).HasPrecision(19, 3);

            builder.HasIndex(allocation => new { allocation.AncillaryReservationUnitId, allocation.ResourceKind, allocation.InventorySourceId }).IsUnique();
            builder.HasIndex(allocation => new { allocation.ResourceKind, allocation.InventorySourceId });
        }
    }
}
