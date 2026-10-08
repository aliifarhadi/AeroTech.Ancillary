using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Configurations
{
    public sealed class AncillaryProvisionCustomerTypeReadModelConfiguration : IEntityTypeConfiguration<AncillaryProvisionCustomerTypeReadModel>
    {
        public void Configure(EntityTypeBuilder<AncillaryProvisionCustomerTypeReadModel> builder)
        {
            builder.ToTable("AncillaryProvisionCustomerTypes");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.HasIndex(row => row.AncillaryProvisionId);
        }
    }
}
