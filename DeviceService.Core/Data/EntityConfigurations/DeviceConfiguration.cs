using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceService.Core.Data.EntityConfigurations
{
    public class SmartloanRequestLogsConfiguration : IEntityTypeConfiguration<SmartloanRequestLogs>
    {
        public void Configure(EntityTypeBuilder<SmartloanRequestLogs> builder)
        {
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasColumnName("ID");
            builder.Property(t => t.CifId).HasColumnName("CIF_ID");

            builder.ToTable("MOBILEAPP_SMART_LOAN_LOGS", "EBANKUSER");
        }
    }
}
