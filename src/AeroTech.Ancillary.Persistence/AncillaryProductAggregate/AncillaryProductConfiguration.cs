using AeroTech.Ancillary.Domain.AncillaryProductAggregate;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryProductAggregate
{
    public sealed class AncillaryProductConfiguration : IEntityTypeConfiguration<AncillaryProduct>
    {
        public void Configure(EntityTypeBuilder<AncillaryProduct> builder)
        {
            builder.ToTable("AncillaryProducts");
            builder.HasKey(product => product.Id);
            builder.Property(product => product.Id).ValueGeneratedNever();

            builder.Property(product => product.ProductRef).HasMaxLength(20).IsRequired();
            builder.Property(product => product.Name).HasMaxLength(100).IsRequired();
            builder.Property(product => product.Description).HasMaxLength(500);

            builder.OwnsOne(product => product.Quantity, quantity =>
            {
                quantity.Property(value => value.Unit).HasColumnName("QuantityUnit");
                quantity.Property(value => value.Min).HasColumnName("QuantityMin");
                quantity.Property(value => value.Max).HasColumnName("QuantityMax");
            });

            builder.OwnsOne(product => product.Document, document =>
            {
                document.Property(value => value.Type).HasColumnName("DocumentType");
                document.Property(value => value.Rfisc).HasColumnName("Rfisc").HasMaxLength(3);
                document.Property(value => value.Rfic).HasColumnName("Rfic").HasMaxLength(1);
            });

            builder.OwnsOne(product => product.Codes, codes =>
            {
                codes.Property(value => value.ServiceTypeCode).HasColumnName("ServiceTypeCode").HasMaxLength(1).IsRequired();
                codes.Property(value => value.GroupCode).HasColumnName("GroupCode").HasMaxLength(2).IsRequired();
                codes.Property(value => value.SubGroupCode).HasColumnName("SubGroupCode").HasMaxLength(2);
                codes.Property(value => value.Description1Code).HasColumnName("Description1Code").HasMaxLength(2);
                codes.Property(value => value.Description2Code).HasColumnName("Description2Code").HasMaxLength(2);
            });

            builder.OwnsOne(product => product.Terms, terms =>
            {
                terms.Property(value => value.Refundable).HasColumnName("Refundable");
                terms.Property(value => value.Commissionable).HasColumnName("Commissionable");
                terms.Property(value => value.Reusable).HasColumnName("Reusable");
                terms.Property(value => value.FormOfRefundCode).HasColumnName("FormOfRefundCode").HasMaxLength(10);
                terms.Property(value => value.InterlineSettlementAllowed).HasColumnName("InterlineSettlementAllowed");
            });

            builder.OwnsOne(product => product.Baggage, baggage =>
            {
                baggage.Property(value => value.Pieces).HasColumnName("BaggagePieces");
                baggage.Property(value => value.Weight).HasColumnName("BaggageWeight");
                baggage.Property(value => value.WeightUnit).HasColumnName("BaggageWeightUnit");
            });

            builder.Navigation(product => product.Quantity).IsRequired();
            builder.Navigation(product => product.Document).IsRequired();
            builder.Navigation(product => product.Codes).IsRequired();
            builder.Navigation(product => product.Terms).IsRequired();

            builder.HasIndex(product => new { product.OwnerAirlineId, product.ProductRef, product.Version }).IsUnique();
            builder.HasIndex(product => new { product.OwnerAirlineId, product.ProductRef }, "IX_AncillaryProducts_OwnerAirlineId_ProductRef_Offered")
                .IsUnique()
                .HasFilter($"[Status] IN ({(int)AncillaryProductStatus.Active}, {(int)AncillaryProductStatus.Suspended})");
            builder.HasIndex(product => new { product.OwnerAirlineId, product.ProductRef }, "IX_AncillaryProducts_OwnerAirlineId_ProductRef_Draft")
                .IsUnique()
                .HasFilter($"[Status] = {(int)AncillaryProductStatus.Draft}");
            builder.HasIndex(product => product.Status);
        }
    }
}
