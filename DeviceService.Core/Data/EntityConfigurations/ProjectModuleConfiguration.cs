using DeviceService.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceService.Core.Data.EntityConfigurations
{
    public class ProjectModuleConfiguration : IEntityTypeConfiguration<ProjectModule>
    {
        public void Configure(EntityTypeBuilder<ProjectModule> builder)
        {
            builder.HasKey(a => a.ProjectModuleId);
            builder.Property(a => a.ProjectModuleId).HasColumnName("ProjectModuleId").ValueGeneratedOnAdd().UseIdentityColumn().IsRequired(true);
            builder.Property(a => a.ProjectModuleName).HasColumnName("ProjectModuleName");
            builder.Property(a => a.CreatedAt).HasColumnName("CreatedAt").IsRequired(true);

            builder.ToTable("ProjectModule");
        }
    }
}
