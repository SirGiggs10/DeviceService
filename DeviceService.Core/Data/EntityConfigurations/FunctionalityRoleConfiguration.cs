using DeviceService.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceService.Core.Data.EntityConfigurations
{
    public class FunctionalityRoleConfiguration : IEntityTypeConfiguration<FunctionalityRole>
    {
        public void Configure(EntityTypeBuilder<FunctionalityRole> builder)
        {
            builder.HasKey(a => new { a.FunctionalityId, a.RoleId });
            builder.Property(a => a.FunctionalityId).HasColumnName("FunctionalityId").IsRequired(true);
            builder.Property(a => a.FunctionalityName).HasColumnName("FunctionalityName").IsRequired(true);
            builder.Property(a => a.RoleId).HasColumnName("RoleId").IsRequired(true);
            builder.Property(a => a.RoleName).HasColumnName("RoleName").IsRequired(true);
            builder.Property(a => a.CreatedAt).HasColumnName("CreatedAt").IsRequired(true);

            builder.ToTable("FunctionalityRole");

            builder.HasOne(a => a.Functionality).WithMany(b => b.FunctionalityRoles).HasForeignKey(c => c.FunctionalityId).IsRequired(true);
            builder.HasOne(a => a.Role).WithMany(b => b.FunctionalityRoles).HasForeignKey(c => c.RoleId).IsRequired(true);
        }
    }
}

