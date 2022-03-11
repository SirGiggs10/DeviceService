using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using DeviceService.Core.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeviceService.Core.Data.EntityConfigurations
{
    public class AuditReportActivityConfiguration : IEntityTypeConfiguration<AuditReportActivity>
    {
        public void Configure(EntityTypeBuilder<AuditReportActivity> builder)
        {
            builder.HasKey(a => a.AuditReportActivityId);
            builder.Property(a => a.AuditReportActivityId).HasColumnName("AuditReportActivityId").ValueGeneratedOnAdd().UseIdentityColumn().IsRequired(true);
            builder.Property(a => a.FunctionalityId).HasColumnName("FunctionalityId");
            builder.Property(a => a.AuditReportActivityDescription).HasColumnName("AuditReportActivityDescription");
            builder.Property(a => a.AuditReportActivityViewUrl).HasColumnName("AuditReportActivityViewUrl");
            builder.Property(a => a.FrontendRoute).HasColumnName("FrontendRoute");
            builder.Property(a => a.CreatedAt).HasColumnName("CreatedAt").IsRequired(true);

            builder.ToTable("AuditReportActivity");

            builder.HasOne(a => a.Functionality).WithOne(b => b.AuditReportActivity).HasForeignKey<AuditReportActivity>(c => c.FunctionalityId).IsRequired(true);
        }
    }
}

