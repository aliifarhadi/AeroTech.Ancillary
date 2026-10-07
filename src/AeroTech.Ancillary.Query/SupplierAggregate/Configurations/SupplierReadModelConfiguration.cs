using AeroTech.Ancillary.Query.SupplierAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.SupplierAggregate.Configurations
{
    public sealed class SupplierReadModelConfiguration : IEntityTypeConfiguration<SupplierReadModel>
    {
        public void Configure(EntityTypeBuilder<SupplierReadModel> builder)
        {
            builder.ToTable("Suppliers");
            builder.HasKey(supplier => supplier.Id);
            builder.Property(supplier => supplier.Id).ValueGeneratedNever();

            builder.Property(supplier => supplier.Name).HasMaxLength(100).IsRequired();
            builder.Property(supplier => supplier.FulfillmentProviderKey).HasMaxLength(50);
        }
    }
}
