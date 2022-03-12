using DeviceService.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceService.Core.Data.EntityConfigurations
{
    public class FunctionalityConfiguration : IEntityTypeConfiguration<Functionality>
    {
        public void Configure(EntityTypeBuilder<Functionality> builder)
        {
            builder.HasKey(a => a.FunctionalityId);
            builder.Property(a => a.FunctionalityId).HasColumnName("FunctionalityId").ValueGeneratedOnAdd().UseIdentityColumn().IsRequired(true);
            builder.Property(a => a.FunctionalityName).HasColumnName("FunctionalityName").IsRequired(true);
            builder.Property(a => a.FunctionalityDescription).HasColumnName("FunctionalityDescription");
            builder.Property(a => a.ProjectModuleId).HasColumnName("ProjectModuleId").IsRequired(true);
            builder.Property(a => a.CreatedAt).HasColumnName("CreatedAt").IsRequired(true);

            builder.HasIndex(a => a.FunctionalityName).IsUnique(true);

            builder.ToTable("Functionality");

            builder.HasOne(a => a.ProjectModule).WithMany(b => b.Functionalities).HasForeignKey(c => c.ProjectModuleId).IsRequired(true);
        }
    }
}
