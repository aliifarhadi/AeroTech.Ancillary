using AeroTech.Ancillary.Query.ServiceSubCodeAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.ServiceSubCodeAggregate.Configurations
{
    public sealed class ServiceSubCodeReadModelConfiguration : IEntityTypeConfiguration<ServiceSubCodeReadModel>
    {
        public void Configure(EntityTypeBuilder<ServiceSubCodeReadModel> builder)
        {
            builder.ToTable("ServiceSubCodes");
            builder.HasKey(subCode => subCode.Id);
            builder.Property(subCode => subCode.Id).ValueGeneratedNever();

            builder.Property(subCode => subCode.Code).HasMaxLength(3).IsRequired();
            builder.Property(subCode => subCode.Rfic).HasMaxLength(1).IsRequired();
            builder.Property(subCode => subCode.GroupCode).HasMaxLength(2).IsRequired();
            builder.Property(subCode => subCode.SubGroupCode).HasMaxLength(2);
            builder.Property(subCode => subCode.Description1Code).HasMaxLength(2);
            builder.Property(subCode => subCode.Description2Code).HasMaxLength(2);
            builder.Property(subCode => subCode.CommercialName).HasMaxLength(30).IsRequired();

            builder.HasIndex(subCode => new { subCode.OwnerAirlineId, subCode.Code });
        }
    }
}
