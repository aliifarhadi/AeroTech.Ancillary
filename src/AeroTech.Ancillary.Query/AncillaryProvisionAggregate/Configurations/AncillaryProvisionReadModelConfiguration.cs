using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Configurations
{
    public sealed class AncillaryProvisionReadModelConfiguration : IEntityTypeConfiguration<AncillaryProvisionReadModel>
    {
        public void Configure(EntityTypeBuilder<AncillaryProvisionReadModel> builder)
        {
            builder.ToTable("AncillaryProvisions");
            builder.HasKey(provision => provision.Id);
            builder.Property(provision => provision.Id).ValueGeneratedNever();

            builder.PrimitiveCollection(provision => provision.FlightNumbers).HasColumnType("nvarchar(max)");
            builder.PrimitiveCollection(provision => provision.FareBasisCodes).HasColumnType("nvarchar(max)");
            builder.PrimitiveCollection(provision => provision.SeatNumbers).HasColumnType("nvarchar(max)");
            builder.PrimitiveCollection(provision => provision.SeatCharacteristicCodes).HasColumnType("nvarchar(max)");
            builder.Property(provision => provision.BaggageWeight).HasPrecision(9, 2);
            builder.Property(provision => provision.FulfillmentProviderKey).HasMaxLength(50).IsRequired();

            builder.HasIndex(provision => new { provision.ServiceDefinitionId, provision.Status });
        }
    }
}
