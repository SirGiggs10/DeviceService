﻿// <auto-generated />
using System;
using DeviceService.Core.Data.DataContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DeviceService.Core.Data.Migrations
{
    [DbContext(typeof(DeviceContext))]
    partial class DeviceContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.15")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("DeviceService.Core.Entities.AuditReport", b =>
                {
                    b.Property<int>("AuditReportId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("AuditReportId")
                        .HasAnnotation("SqlServer:IdentityIncrement", 1)
                        .HasAnnotation("SqlServer:IdentitySeed", 1)
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("AuditReportActivityId")
                        .HasColumnType("int")
                        .HasColumnName("AuditReportActivityId");

                    b.Property<string>("AuditReportActivityResourceId")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("AuditReportActivityResourceId");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("CreatedAt");

                    b.Property<int>("UserId")
                        .HasColumnType("int")
                        .HasColumnName("UserId");

                    b.HasKey("AuditReportId");

                    b.HasIndex("AuditReportActivityId");

                    b.HasIndex("UserId");

                    b.ToTable("AuditReport");
                });

            modelBuilder.Entity("DeviceService.Core.Entities.AuditReportActivity", b =>
                {
                    b.Property<int>("AuditReportActivityId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("AuditReportActivityId")
                        .HasAnnotation("SqlServer:IdentityIncrement", 1)
                        .HasAnnotation("SqlServer:IdentitySeed", 1)
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("AuditReportActivityDescription")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("AuditReportActivityDescription");

                    b.Property<string>("AuditReportActivityViewUrl")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("AuditReportActivityViewUrl");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("CreatedAt");

                    b.Property<string>("FrontendRoute")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("FrontendRoute");

                    b.Property<int>("FunctionalityId")
                        .HasColumnType("int")
                        .HasColumnName("FunctionalityId");

                    b.HasKey("AuditReportActivityId");

                    b.HasIndex("FunctionalityId")
                        .IsUnique();

                    b.ToTable("AuditReportActivity");
                });

            modelBuilder.Entity("DeviceService.Core.Entities.Device", b =>
                {
                    b.Property<int>("DeviceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("DeviceId")
                        .HasAnnotation("SqlServer:IdentityIncrement", 1)
                        .HasAnnotation("SqlServer:IdentitySeed", 1)
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("CreatedAt");

                    b.Property<string>("DeviceIconFileName")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("DeviceIconFileName");

                    b.Property<string>("DeviceIconPublicId")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("DeviceIconPublicId");

                    b.Property<string>("DeviceIconUrl")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("DeviceIconUrl");

                    b.Property<string>("DeviceName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("DeviceName");

                    b.Property<int>("DeviceTypeId")
                        .HasColumnType("int")
                        .HasColumnName("DeviceTypeId");

                    b.Property<string>("Status")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("Status");

                    b.Property<double>("Temperature")
                        .HasColumnType("float")
                        .HasColumnName("Temperature");

                    b.Property<double>("TotalUsageTimeInHours")
                        .HasColumnType("float")
                        .HasColumnName("TotalUsageTimeInHours");

                    b.Property<int>("UserId")
                        .HasColumnType("int")
                        .HasColumnName("UserId");

                    b.HasKey("DeviceId");

                    b.HasIndex("DeviceTypeId");

                    b.HasIndex("UserId");

                    b.ToTable("Device");
                });

            modelBuilder.Entity("DeviceService.Core.Entities.DeviceOperation", b =>
                {
                    b.Property<int>("DeviceOperationId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("DeviceOperationId")
                        .HasAnnotation("SqlServer:IdentityIncrement", 1)
                        .HasAnnotation("SqlServer:IdentitySeed", 1)
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("CreatedAt");

                    b.Property<string>("DeviceOperationName")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("DeviceOperationName");

                    b.HasKey("DeviceOperationId");

                    b.HasIndex("DeviceOperationName")
                        .IsUnique();

                    b.ToTable("DeviceOperation");
                });

            modelBuilder.Entity("DeviceService.Core.Entities.DeviceType", b =>
                {
                    b.Property<int>("DeviceTypeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("DeviceTypeId")
                        .HasAnnotation("SqlServer:IdentityIncrement", 1)
                        .HasAnnotation("SqlServer:IdentitySeed", 1)
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("CreatedAt");

                    b.Property<string>("DeviceTypeName")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("DeviceTypeName");

                    b.HasKey("DeviceTypeId");

                    b.HasIndex("DeviceTypeName")
                        .IsUnique();

                    b.ToTable("DeviceType");
                });

            modelBuilder.Entity("DeviceService.Core.Entities.DeviceTypeOperation", b =>
                {
                    b.Property<int>("DeviceTypeId")
                        .HasColumnType("int")
                        .HasColumnName("DeviceTypeId");

                    b.Property<int>("DeviceOperationId")
                        .HasColumnType("int")
                        .HasColumnName("DeviceOperationId");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("CreatedAt");

                    b.HasKey("DeviceTypeId", "DeviceOperationId");

                    b.HasIndex("DeviceOperationId");

                    b.ToTable("DeviceTypeOperation");
                });

            modelBuilder.Entity("DeviceService.Core.Entities.Functionality", b =>
                {
                    b.Property<int>("FunctionalityId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("FunctionalityId")
                        .HasAnnotation("SqlServer:IdentityIncrement", 1)
                        .HasAnnotation("SqlServer:IdentitySeed", 1)
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("CreatedAt");

                    b.Property<string>("FunctionalityDescription")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("FunctionalityDescription");

                    b.Property<string>("FunctionalityName")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("FunctionalityName");

                    b.Property<int>("ProjectModuleId")
                        .HasColumnType("int")
                        .HasColumnName("ProjectModuleId");

                    b.HasKey("FunctionalityId");

                    b.HasIndex("FunctionalityName")
                        .IsUnique();

                    b.HasIndex("ProjectModuleId");

                    b.ToTable("Functionality");
                });

            modelBuilder.Entity("DeviceService.Core.Entities.FunctionalityRole", b =>
                {
                    b.Property<int>("FunctionalityId")
                        .HasColumnType("int")
                        .HasColumnName("FunctionalityId");

                    b.Property<int>("RoleId")
                        .HasColumnType("int")
                        .HasColumnName("RoleId");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("CreatedAt");

                    b.Property<string>("FunctionalityName")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("FunctionalityName");

                    b.Property<string>("RoleName")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("RoleName");

                    b.HasKey("FunctionalityId", "RoleId");

                    b.HasIndex("RoleId");

                    b.HasIndex("FunctionalityName", "RoleName")
                        .IsUnique();

                    b.ToTable("FunctionalityRole");
                });

            modelBuilder.Entity("DeviceService.Core.Entities.ProjectModule", b =>
                {
                    b.Property<int>("ProjectModuleId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("ProjectModuleId")
                        .HasAnnotation("SqlServer:IdentityIncrement", 1)
                        .HasAnnotation("SqlServer:IdentitySeed", 1)
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("CreatedAt");

                    b.Property<string>("ProjectModuleName")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("ProjectModuleName");

                    b.HasKey("ProjectModuleId");

                    b.HasIndex("ProjectModuleName")
                        .IsUnique();

                    b.ToTable("ProjectModule");
                });

            modelBuilder.Entity("DeviceService.Core.Entities.Role", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset?>("ModifiedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("RoleDescription")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RoleName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UserType")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("DeviceService.Core.Entities.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("Address")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<bool>("Deleted")
                        .HasColumnType("bit");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("FullName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset?>("LastLoginDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("SecondToLastLoginDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<int>("UserType")
                        .HasColumnType("int");

                    b.Property<int>("UserTypeId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("DeviceService.Core.Entities.UserRole", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<int>("RoleId")
                        .HasColumnType("int");

                    b.Property<int?>("RoleId1")
                        .HasColumnType("int");

                    b.Property<int?>("UserId1")
                        .HasColumnType("int");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.HasIndex("RoleId1");

                    b.HasIndex("UserId1");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<int>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("RoleId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<int>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<int>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<int>", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("DeviceService.Core.Entities.AuditReport", b =>
                {
                    b.HasOne("DeviceService.Core.Entities.AuditReportActivity", "AuditReportActivity")
                        .WithMany("AuditReports")
                        .HasForeignKey("AuditReportActivityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DeviceService.Core.Entities.User", "User")
                        .WithMany("AuditReports")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AuditReportActivity");

                    b.Navigation("User");
                });

            modelBuilder.Entity("DeviceService.Core.Entities.AuditReportActivity", b =>
                {
                    b.HasOne("DeviceService.Core.Entities.Functionality", "Functionality")
                        .WithOne("AuditReportActivity")
                        .HasForeignKey("DeviceService.Core.Entities.AuditReportActivity", "FunctionalityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Functionality");
                });

            modelBuilder.Entity("DeviceService.Core.Entities.Device", b =>
                {
                    b.HasOne("DeviceService.Core.Entities.DeviceType", "DeviceType")
                        .WithMany("Devices")
                        .HasForeignKey("DeviceTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DeviceService.Core.Entities.User", "User")
                        .WithMany("Devices")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DeviceType");

                    b.Navigation("User");
                });

            modelBuilder.Entity("DeviceService.Core.Entities.DeviceTypeOperation", b =>
                {
                    b.HasOne("DeviceService.Core.Entities.DeviceOperation", "DeviceOperation")
                        .WithMany("DeviceTypeOperations")
                        .HasForeignKey("DeviceOperationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DeviceService.Core.Entities.DeviceType", "DeviceType")
                        .WithMany("DeviceTypeOperations")
                        .HasForeignKey("DeviceTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DeviceOperation");

                    b.Navigation("DeviceType");
                });

            modelBuilder.Entity("DeviceService.Core.Entities.Functionality", b =>
                {
                    b.HasOne("DeviceService.Core.Entities.ProjectModule", "ProjectModule")
                        .WithMany("Functionalities")
                        .HasForeignKey("ProjectModuleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ProjectModule");
                });

            modelBuilder.Entity("DeviceService.Core.Entities.FunctionalityRole", b =>
                {
                    b.HasOne("DeviceService.Core.Entities.Functionality", "Functionality")
                        .WithMany("FunctionalityRoles")
                        .HasForeignKey("FunctionalityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DeviceService.Core.Entities.Role", "Role")
                        .WithMany("FunctionalityRoles")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Functionality");

                    b.Navigation("Role");
                });

            modelBuilder.Entity("DeviceService.Core.Entities.UserRole", b =>
                {
                    b.HasOne("DeviceService.Core.Entities.Role", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DeviceService.Core.Entities.Role", "Role")
                        .WithMany("UserRoles")
                        .HasForeignKey("RoleId1");

                    b.HasOne("DeviceService.Core.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DeviceService.Core.Entities.User", "User")
                        .WithMany("UserRoles")
                        .HasForeignKey("UserId1");

                    b.Navigation("Role");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<int>", b =>
                {
                    b.HasOne("DeviceService.Core.Entities.Role", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<int>", b =>
                {
                    b.HasOne("DeviceService.Core.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<int>", b =>
                {
                    b.HasOne("DeviceService.Core.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<int>", b =>
                {
                    b.HasOne("DeviceService.Core.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("DeviceService.Core.Entities.AuditReportActivity", b =>
                {
                    b.Navigation("AuditReports");
                });

            modelBuilder.Entity("DeviceService.Core.Entities.DeviceOperation", b =>
                {
                    b.Navigation("DeviceTypeOperations");
                });

            modelBuilder.Entity("DeviceService.Core.Entities.DeviceType", b =>
                {
                    b.Navigation("Devices");

                    b.Navigation("DeviceTypeOperations");
                });

            modelBuilder.Entity("DeviceService.Core.Entities.Functionality", b =>
                {
                    b.Navigation("AuditReportActivity");

                    b.Navigation("FunctionalityRoles");
                });

            modelBuilder.Entity("DeviceService.Core.Entities.ProjectModule", b =>
                {
                    b.Navigation("Functionalities");
                });

            modelBuilder.Entity("DeviceService.Core.Entities.Role", b =>
                {
                    b.Navigation("FunctionalityRoles");

                    b.Navigation("UserRoles");
                });

            modelBuilder.Entity("DeviceService.Core.Entities.User", b =>
                {
                    b.Navigation("AuditReports");

                    b.Navigation("Devices");

                    b.Navigation("UserRoles");
                });
#pragma warning restore 612, 618
        }
    }
}
