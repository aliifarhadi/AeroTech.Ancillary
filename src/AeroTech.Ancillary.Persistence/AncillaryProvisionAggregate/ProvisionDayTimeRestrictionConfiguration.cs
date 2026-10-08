using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryProvisionAggregate
{
    public sealed class ProvisionDayTimeRestrictionConfiguration : IEntityTypeConfiguration<ProvisionDayTimeRestriction>
    {
        public void Configure(EntityTypeBuilder<ProvisionDayTimeRestriction> builder)
        {
            builder.ToTable("ProvisionDayTimeRestrictions");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.HasIndex(row => new { row.AncillaryProvisionId, row.DayOfWeek, row.StartTime, row.EndTime, row.Effect }).IsUnique();
        }
    }
}
