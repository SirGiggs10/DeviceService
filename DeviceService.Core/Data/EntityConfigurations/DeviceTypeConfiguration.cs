using DeviceService.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceService.Core.Data.EntityConfigurations
{
    public class DeviceTypeConfiguration : IEntityTypeConfiguration<DeviceType>
    {   
        public void Configure(EntityTypeBuilder<DeviceType> builder)
        {
            builder.HasKey(a => a.DeviceTypeId);
            builder.Property(a => a.DeviceTypeId).HasColumnName("DeviceTypeId").ValueGeneratedOnAdd().UseIdentityColumn().IsRequired(true);
            builder.Property(a => a.DeviceTypeName).HasColumnName("DeviceTypeName").IsRequired(true);
            builder.Property(a => a.CreatedAt).HasColumnName("CreatedAt").IsRequired(true);

            builder.HasIndex(a => a.DeviceTypeName).IsUnique(true);

            builder.ToTable("DeviceType");
        }
    }
}
