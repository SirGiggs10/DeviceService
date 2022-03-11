using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using DeviceService.Core.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeviceService.Core.Data.EntityConfigurations
{
    public class AuditReportConfiguration : IEntityTypeConfiguration<AuditReport>
    {
        public void Configure(EntityTypeBuilder<AuditReport> builder)
        {
            builder.HasKey(a => a.AuditReportActivityId);
            builder.Property(a => a.AuditReportId).HasColumnName("AuditReportId").ValueGeneratedOnAdd().UseIdentityColumn().IsRequired(true);
            builder.Property(a => a.AuditReportActivityId).HasColumnName("AuditReportActivityId");
            builder.Property(a => a.UserId).HasColumnName("UserId");
            builder.Property(a => a.AuditReportActivityResourceId).HasColumnName("AuditReportActivityResourceId");
            builder.Property(a => a.CreatedAt).HasColumnName("CreatedAt").IsRequired(true);

            builder.ToTable("AuditReport");

            builder.HasOne(a => a.AuditReportActivity).WithMany(b => b.AuditReports).HasForeignKey(c => c.AuditReportActivityId).IsRequired(true);
            builder.HasOne(a => a.User).WithMany(b => b.AuditReports).HasForeignKey(c => c.UserId).IsRequired(true);
        }
    }
}

