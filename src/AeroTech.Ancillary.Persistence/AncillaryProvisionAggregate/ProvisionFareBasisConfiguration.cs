using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryProvisionAggregate
{
    public sealed class ProvisionFareBasisConfiguration : IEntityTypeConfiguration<ProvisionFareBasis>
    {
        public void Configure(EntityTypeBuilder<ProvisionFareBasis> builder)
        {
            builder.ToTable("ProvisionFareBases");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.Property(row => row.FareBasisCode).HasMaxLength(64).IsRequired();
            builder.HasIndex(row => new { row.AncillaryProvisionId, row.FareBasisCode }).IsUnique();
            builder.HasOne<AncillaryProvision>()
                .WithMany()
                .HasForeignKey(row => row.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
