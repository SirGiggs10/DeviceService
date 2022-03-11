using DeviceService.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceService.Core.Data.EntityConfigurations
{
    public class DeviceTypeOperationConfiguration : IEntityTypeConfiguration<DeviceTypeOperation>
    {
        public void Configure(EntityTypeBuilder<DeviceTypeOperation> builder)
        {
            builder.HasKey(a => new { a.DeviceTypeId, a.DeviceOperationId });
            builder.Property(a => a.DeviceTypeId).HasColumnName("DeviceTypeId").IsRequired(true);
            builder.Property(a => a.DeviceOperationId).HasColumnName("DeviceOperationId").IsRequired(true);
            builder.Property(a => a.CreatedAt).HasColumnName("CreatedAt").IsRequired(true);

            builder.ToTable("DeviceTypeOperation");

            builder.HasOne(a => a.DeviceType).WithMany(b => b.DeviceTypeOperations).HasForeignKey(c => c.DeviceTypeId).IsRequired(true);
            builder.HasOne(a => a.DeviceOperation).WithMany(b => b.DeviceTypeOperations).HasForeignKey(c => c.DeviceOperationId).IsRequired(true);
        }
    }
}

