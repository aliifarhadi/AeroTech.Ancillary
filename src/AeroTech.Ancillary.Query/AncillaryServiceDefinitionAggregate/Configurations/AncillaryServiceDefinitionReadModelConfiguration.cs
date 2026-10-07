using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Configurations
{
    public sealed class AncillaryServiceDefinitionReadModelConfiguration : IEntityTypeConfiguration<AncillaryServiceDefinitionReadModel>
    {
        public void Configure(EntityTypeBuilder<AncillaryServiceDefinitionReadModel> builder)
        {
            builder.ToTable("AncillaryServiceDefinitions");
            builder.HasKey(definition => definition.Id);
            builder.Property(definition => definition.Id).ValueGeneratedNever();

            builder.Property(definition => definition.SupplierName).HasMaxLength(100).IsRequired();
            builder.Property(definition => definition.ServiceDefinitionRef).HasMaxLength(30).IsRequired();
            builder.Property(definition => definition.ServiceTypeCode).HasMaxLength(1).IsRequired();
            builder.Property(definition => definition.ServiceSubCode).HasMaxLength(3).IsRequired();
            builder.Property(definition => definition.GroupCode).HasMaxLength(2).IsRequired();
            builder.Property(definition => definition.SubGroupCode).HasMaxLength(2);
            builder.Property(definition => definition.Description1Code).HasMaxLength(2);
            builder.Property(definition => definition.Description2Code).HasMaxLength(2);
            builder.Property(definition => definition.CommercialName).HasMaxLength(100).IsRequired();
            builder.Property(definition => definition.Description).HasMaxLength(500);
            builder.Property(definition => definition.DocumentRfic).HasMaxLength(1);
            builder.Property(definition => definition.DocumentRfisc).HasMaxLength(3);
            builder.Property(definition => definition.BookingSsrCode).HasMaxLength(4);
            builder.Property(definition => definition.BookingSsimCode).HasMaxLength(4);

            builder.HasIndex(definition => new { definition.OwnerAirlineId, definition.Status });
            builder.HasIndex(definition => definition.SupplierId);
        }
    }
}
