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
            builder.Property(snapshot => snapshot.OfferId).HasMaxLength(512);
            builder.Ignore(snapshot => snapshot.SourceKind);
            builder.HasMany(snapshot => snapshot.Lines)
                .WithOne()
                .HasForeignKey(line => line.SelectionSnapshotId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(snapshot => snapshot.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
