using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryProvisionAggregate
{
    public sealed class ProvisionRbdConfiguration : IEntityTypeConfiguration<ProvisionRbd>
    {
        public void Configure(EntityTypeBuilder<ProvisionRbd> builder)
        {
            builder.ToTable("ProvisionRbds");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.HasIndex(row => new { row.AncillaryProvisionId, row.RbdId }).IsUnique();
        }
    }
}
