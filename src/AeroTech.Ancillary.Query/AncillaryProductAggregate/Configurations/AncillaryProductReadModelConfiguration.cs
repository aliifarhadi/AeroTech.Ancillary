using AeroTech.Ancillary.Query.AncillaryProductAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.AncillaryProductAggregate.Configurations
{
    public sealed class AncillaryProductReadModelConfiguration : IEntityTypeConfiguration<AncillaryProductReadModel>
    {
        public void Configure(EntityTypeBuilder<AncillaryProductReadModel> builder)
        {
            builder.ToTable("AncillaryProducts");
            builder.HasKey(product => product.Id);
            builder.Property(product => product.Id).ValueGeneratedNever();

            builder.Property(product => product.ProductRef).HasMaxLength(20).IsRequired();
            builder.Property(product => product.Name).HasMaxLength(100).IsRequired();
            builder.Property(product => product.Description).HasMaxLength(500);
            builder.Property(product => product.Rfisc).HasMaxLength(3);
            builder.Property(product => product.Rfic).HasMaxLength(1);
            builder.Property(product => product.ServiceTypeCode).HasMaxLength(1).IsRequired();
            builder.Property(product => product.GroupCode).HasMaxLength(2).IsRequired();
            builder.Property(product => product.SubGroupCode).HasMaxLength(2);
            builder.Property(product => product.Description1Code).HasMaxLength(2);
            builder.Property(product => product.Description2Code).HasMaxLength(2);
            builder.Property(product => product.FormOfRefundCode).HasMaxLength(10);

            builder.HasIndex(product => new { product.OwnerAirlineId, product.ProductRef, product.Version });
        }
    }
}
