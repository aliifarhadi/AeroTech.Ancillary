using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Configurations
{
    public sealed class AncillaryProvisionPermittedTravelPeriodReadModelConfiguration : IEntityTypeConfiguration<AncillaryProvisionPermittedTravelPeriodReadModel>
    {
        public void Configure(EntityTypeBuilder<AncillaryProvisionPermittedTravelPeriodReadModel> builder)
        {
            builder.ToTable("AncillaryProvisionPermittedTravelPeriods");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.HasIndex(row => row.AncillaryProvisionId);
        }
    }
}
