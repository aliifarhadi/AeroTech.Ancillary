using AeroTech.Ancillary.Domain.SelectionSnapshotAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.SelectionSnapshotAggregate
{
    public sealed class SelectionSnapshotLineConfiguration : IEntityTypeConfiguration<SelectionSnapshotLine>
    {
        public void Configure(EntityTypeBuilder<SelectionSnapshotLine> builder)
        {
            builder.ToTable("SelectionSnapshotLines");
            builder.HasKey(line => new { line.SelectionSnapshotId, line.Sequence });
            builder.Property(line => line.SelectionSnapshotId).HasColumnType("varchar(24)");
            builder.Property(line => line.Sequence).ValueGeneratedNever();

            builder.Property(line => line.OfferItemId).HasColumnType("varchar(1024)").IsRequired();
            builder.Property(line => line.CoverageScope).HasConversion<short>().HasColumnType("smallint");
            builder.Property(line => line.TravellerRef).HasMaxLength(100);
            builder.Property(line => line.CoverageRef).HasMaxLength(100).IsRequired();
            builder.Property(line => line.OfferBinding).HasColumnType("char(43)").IsRequired();
            builder.Property(line => line.SalesBinding).HasColumnType("char(43)").IsRequired();
            builder.Property(line => line.SelectedJson).HasColumnType("nvarchar(max)");
        }
    }
}
