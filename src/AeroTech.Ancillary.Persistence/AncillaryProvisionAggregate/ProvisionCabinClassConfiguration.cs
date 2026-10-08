using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryProvisionAggregate
{
    public sealed class ProvisionCabinClassConfiguration : IEntityTypeConfiguration<ProvisionCabinClass>
    {
        public void Configure(EntityTypeBuilder<ProvisionCabinClass> builder)
        {
            builder.ToTable("ProvisionCabinClasses");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.HasIndex(row => new { row.AncillaryProvisionId, row.CabinClassId }).IsUnique();
        }
    }
}
