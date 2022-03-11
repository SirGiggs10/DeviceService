using DeviceService.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceService.Core.Data.EntityConfigurations
{
    public class DeviceOperationConfiguration : IEntityTypeConfiguration<DeviceOperation>
    {
        public void Configure(EntityTypeBuilder<DeviceOperation> builder)
        {
            builder.HasKey(a => a.DeviceOperationId);
            builder.Property(a => a.DeviceOperationId).HasColumnName("DeviceOperationId").ValueGeneratedOnAdd().UseIdentityColumn().IsRequired(true);
            builder.Property(a => a.DeviceOperationName).HasColumnName("DeviceOperationName");
            builder.Property(a => a.CreatedAt).HasColumnName("CreatedAt").IsRequired(true);

            builder.ToTable("DeviceOperation");
        }
    }
}
