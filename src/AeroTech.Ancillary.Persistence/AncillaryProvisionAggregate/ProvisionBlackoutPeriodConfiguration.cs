using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryProvisionAggregate
{
    public sealed class ProvisionBlackoutPeriodConfiguration : IEntityTypeConfiguration<ProvisionBlackoutPeriod>
    {
        public void Configure(EntityTypeBuilder<ProvisionBlackoutPeriod> builder)
        {
            builder.ToTable("ProvisionBlackoutPeriods");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.HasIndex(row => new { row.AncillaryProvisionId, row.StartDate, row.EndDate }).IsUnique();
            builder.HasOne<AncillaryProvision>()
                .WithMany()
                .HasForeignKey(row => row.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
