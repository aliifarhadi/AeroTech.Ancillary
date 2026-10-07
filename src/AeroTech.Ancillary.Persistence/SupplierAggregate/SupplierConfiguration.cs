using AeroTech.Ancillary.Domain.SupplierAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.SupplierAggregate
{
    public sealed class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
    {
        public void Configure(EntityTypeBuilder<Supplier> builder)
        {
            builder.ToTable("Suppliers");
            builder.HasKey(supplier => supplier.Id);
            builder.Property(supplier => supplier.Id).ValueGeneratedNever();

            builder.Property(supplier => supplier.Name).HasMaxLength(100).IsRequired();
            builder.Property(supplier => supplier.FulfillmentProviderKey).HasMaxLength(50);
        }
    }
}
