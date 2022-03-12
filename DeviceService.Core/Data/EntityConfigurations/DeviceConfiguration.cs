using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using DeviceService.Core.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeviceService.Core.Data.EntityConfigurations
{
    public class DeviceConfiguration : IEntityTypeConfiguration<Device>
    {
        public void Configure(EntityTypeBuilder<Device> builder)
        {
            builder.HasKey(a => a.DeviceId);
            builder.Property(a => a.DeviceId).HasColumnName("DeviceId").ValueGeneratedOnAdd().UseIdentityColumn().IsRequired(true);
            builder.Property(a => a.DeviceName).HasColumnName("DeviceName").IsRequired(true);
            builder.Property(a => a.UserId).HasColumnName("UserId").IsRequired(true);
            builder.Property(a => a.DeviceTypeId).HasColumnName("DeviceTypeId").IsRequired(true);
            builder.Property(a => a.Status).HasColumnName("Status");
            builder.Property(a => a.Temperature).HasColumnName("Temperature");
            builder.Property(a => a.TotalUsageTimeInHours).HasColumnName("TotalUsageTimeInHours");
            builder.Property(a => a.DeviceIconPublicId).HasColumnName("DeviceIconPublicId");
            builder.Property(a => a.DeviceIconUrl).HasColumnName("DeviceIconUrl");
            builder.Property(a => a.DeviceIconFileName).HasColumnName("DeviceIconFileName");
            builder.Property(a => a.CreatedAt).HasColumnName("CreatedAt").IsRequired(true);

            //builder.ToTable("Device", "DeviceDb");
            builder.ToTable("Device");

            builder.HasOne(a => a.DeviceType).WithMany(b => b.Devices).HasForeignKey(c => c.DeviceTypeId).IsRequired(true);
            builder.HasOne(a => a.User).WithMany(b => b.Devices).HasForeignKey(c => c.UserId).IsRequired(true);
        }
    }
}
