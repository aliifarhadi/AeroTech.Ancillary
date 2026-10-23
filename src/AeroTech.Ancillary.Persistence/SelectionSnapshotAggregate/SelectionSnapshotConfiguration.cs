using AeroTech.Ancillary.Domain.SelectionSnapshotAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.SelectionSnapshotAggregate
{
    public sealed class SelectionSnapshotConfiguration : IEntityTypeConfiguration<SelectionSnapshot>
    {
        public void Configure(EntityTypeBuilder<SelectionSnapshot> builder)
        {
            builder.ToTable("SelectionSnapshots");
            builder.HasKey(snapshot => snapshot.Id);
            builder.Property(snapshot => snapshot.Id).HasColumnType("varchar(24)").ValueGeneratedNever();

            builder.Property(snapshot => snapshot.Version).HasColumnType("smallint");
            builder.Property(snapshot => snapshot.CoverageScope).HasConversion<short>().HasColumnType("smallint");
            builder.Property(snapshot => snapshot.TravellerRef).HasMaxLength(100);
            builder.Property(snapshot => snapshot.CoverageRef).HasMaxLength(100).IsRequired();
            builder.Property(snapshot => snapshot.OfferBinding).HasColumnType("char(43)").IsRequired();
            builder.Property(snapshot => snapshot.SalesBinding).HasColumnType("char(43)").IsRequired();
            builder.Property(snapshot => snapshot.SelectedJson).HasColumnType("nvarchar(max)").IsRequired();
        }
    }
}
