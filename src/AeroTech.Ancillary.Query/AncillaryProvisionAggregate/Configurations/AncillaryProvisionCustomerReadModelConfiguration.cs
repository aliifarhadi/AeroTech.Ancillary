using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Configurations
{
    public sealed class AncillaryProvisionCustomerReadModelConfiguration : IEntityTypeConfiguration<AncillaryProvisionCustomerReadModel>
    {
        public void Configure(EntityTypeBuilder<AncillaryProvisionCustomerReadModel> builder)
        {
            builder.ToTable("AncillaryProvisionCustomers");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.HasIndex(row => row.AncillaryProvisionId);
        }
    }
}
