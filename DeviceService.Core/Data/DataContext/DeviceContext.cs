using DeviceService.Core.Entities;
using DeviceService.Core.Helpers.ConfigurationSettings.ConfigManager;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DeviceService.Core.Data.DataContext
{
    public class DeviceContext : IdentityDbContext<User, Role, int, IdentityUserClaim<int>, UserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        public DeviceContext(DbContextOptions<DeviceContext> dbContextOptions) : base(dbContextOptions)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = ConfigSettings.ConnectionString.DeviceDbConnectionString;

                optionsBuilder.UseSqlServer(connectionString,
                    sqlServerOptionsAction: sqlOptions =>
                    {
                        //sqlOptions.MigrationsAssembly("");
                        //Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency
                        sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                    });
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<AuditReport> AuditReport { get; set; }
        public DbSet<AuditReportActivity> AuditReportActivity { get; set; }
        public DbSet<Device> Device { get; set; }
        public DbSet<DeviceOperation> DeviceOperation { get; set; }
        public DbSet<DeviceType> DeviceType { get; set; }
        public DbSet<DeviceTypeOperation> DeviceTypeOperation { get; set; }
        public DbSet<Functionality> Functionality { get; set; }
        public DbSet<FunctionalityRole> FunctionalityRole { get; set; }
        public DbSet<ProjectModule> ProjectModule { get; set; }
        /*public DbSet<Role> Role { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<UserRole> UserRole { get; set; }*/
    }
}
