using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Configurations
{
    public sealed class AncillaryProvisionPassengerTypeReadModelConfiguration : IEntityTypeConfiguration<AncillaryProvisionPassengerTypeReadModel>
    {
        public void Configure(EntityTypeBuilder<AncillaryProvisionPassengerTypeReadModel> builder)
        {
            builder.ToTable("AncillaryProvisionPassengerTypes");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.HasIndex(row => row.AncillaryProvisionId);
        }
    }
}
