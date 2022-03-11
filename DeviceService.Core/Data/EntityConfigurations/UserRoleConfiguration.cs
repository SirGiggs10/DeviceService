using DeviceService.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceService.Core.Data.EntityConfigurations
{
    public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.HasKey(a => new { a.UserId, a.RoleId });
            builder.Property(a => a.UserId).HasColumnName("UserId").IsRequired(true);
            builder.Property(a => a.RoleId).HasColumnName("RoleId").IsRequired(true);

            builder.ToTable("UserRole");

            builder.HasOne(a => a.User).WithMany(b => b.UserRoles).HasForeignKey(c => c.UserId).IsRequired(true);
            builder.HasOne(a => a.Role).WithMany(b => b.UserRoles).HasForeignKey(c => c.RoleId).IsRequired(true);
        }
    }
}

