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
            builder.Property(t => t.TranID).HasColumnName("TRAN_ID");
            builder.Property(t => t.FundId).HasColumnName("FUND_ID");
            builder.Property(t => t.FirstName).HasColumnName("FIRST_NAME");
            builder.Property(t => t.MiddleName).HasColumnName("MIDDLE_NAME");
            builder.Property(t => t.LastName).HasColumnName("LAST_NAME");
            builder.Property(t => t.EmailAddress).HasColumnName("EMAIL_ADDRESS");
            builder.Property(t => t.PhoneNumber).HasColumnName("PHONE_NUMBER");
            builder.Property(t => t.LoanAmount).HasColumnName("LOAN_AMOUNT");
            builder.Property(t => t.MonthlyAmount).HasColumnName("MONTHLY_AMOUNT");
            builder.Property(t => t.LienAmount).HasColumnName("LIEN_AMOUNT");
            builder.Property(t => t.TotalAmount).HasColumnName("TOTAL_AMOUNT");
            builder.Property(t => t.InterestRate).HasColumnName("INTEREST_RATE");
            builder.Property(t => t.Tenure).HasColumnName("TENURE");
            builder.Property(t => t.Fees).HasColumnName("FEES");
            builder.Property(t => t.EAccountNumber).HasColumnName("EACCOUNT_NUMBER");
            builder.Property(t => t.FundId).HasColumnName("FUND_ID");
            builder.Property(t => t.LoanAccountNumber).HasColumnName("LOAN_ACCOUNT_NUMBER");
            builder.Property(t => t.OperationalAccount).HasColumnName("OPERATIONAL_ACCOUNT_NUMBER");
            builder.Property(t => t.LoanStatus).HasColumnName("LOAN_STATUS");
            builder.Property(t => t.RepaymentDay).HasColumnName("REPAYMENT_DAY");
            builder.Property(t => t.MaturityDate).HasColumnName("LOAN_MATURITY_DATE");
            builder.Property(t => t.CreatedDate).HasColumnName("CREATED_DATE");
            builder.Property(t => t.IndemnityTimestamp).HasColumnName("INDEMNITY_TIMESTAMP");
            builder.Property(t => t.MutualFundsLimit).HasColumnName("MUTUAL_FUND_LIMIT");
            builder.Property(t => t.MutualFundsAvailableBalance).HasColumnName("MUTUAL_FUND_AVAILABLE_BALANCE");
            builder.Property(t => t.MutualFundsBalance).HasColumnName("MUTUAL_FUND_BALANCE");

            builder.Property(t => t.LastEndpointName).HasColumnName("LAST_ENDPOINT_NAME");
            builder.Property(t => t.LastEndpointDescription).HasColumnName("LAST_ENDPOINT_DESC");

            builder.Property(t => t.CreditCheckResponse).HasColumnName("CREDIT_CHECK_RESPONSE");
            builder.Property(t => t.PostLienResponse).HasColumnName("POST_LIEN_RESPONSE");
            builder.Property(t => t.CRMS300Response).HasColumnName("CRMS300_RESPONSE");
            builder.Property(t => t.CreateLoanAccountResponse).HasColumnName("CREATE_LOANACCOUNT_RESPONSE");
            builder.Property(t => t.SanctionLimitResponse).HasColumnName("SANCTION_LIMIT_RESPONSE");
            builder.Property(t => t.LoanDisbursementResponse).HasColumnName("LOAN_DISBURSEMENT_RESPONSE");
            builder.Property(t => t.ChargesResponse).HasColumnName("CHARGES_RESPONSE");
            builder.Property(t => t.ChargesLienResponse).HasColumnName("CHARGES_LIEN_RESPONSE");
            builder.Property(t => t.LienModificationResponse).HasColumnName("LIEN_MODIFICATION_RESPONSE");
            builder.Property(t => t.DisbursementSuccessful).HasColumnName("IS_DISBURSEMENT_SUCCESSFUL");

            builder.Property(t => t.RedboxCRMS300Request).HasColumnName("RBX_CRMS300_REQ");
            builder.Property(t => t.RedboxCRMS300Response).HasColumnName("RBX_CRMS300_RES");
            builder.Property(t => t.RedboxCreateLoanRequest).HasColumnName("RBX_CREATE_LOAN_REQ");
            builder.Property(t => t.RedboxCreateLoanResponse).HasColumnName("RBX_CREATE_LOAN_RES");
            builder.Property(t => t.RedboxSanctionLimitRequest).HasColumnName("RBX_SANCTION_LIMIT_REQ");
            builder.Property(t => t.RedboxSanctionLimitResponse).HasColumnName("RBX_SANCTION_LIMIT_RES");
            builder.Property(t => t.RedboxDisbursementRequest).HasColumnName("RBX_DISB_REQ");
            builder.Property(t => t.RedboxDisbursementResponse).HasColumnName("RBX_DISB_RES");
            builder.Property(t => t.RedboxChargesRequest).HasColumnName("RBX_CHARGES_REQ");
            builder.Property(t => t.RedboxChargesResponse).HasColumnName("RBX_CHARGES_RESP");
            builder.Property(t => t.RedboxChargesLienRequest).HasColumnName("RBX_CHARGES_LIEN_REQ");
            builder.Property(t => t.RedboxChargesLienResponse).HasColumnName("RBX_CHARGES_LIEN_RES");
            builder.Property(t => t.LienPostingRequest).HasColumnName("HTTP_LIEN_POST_REQ");
            builder.Property(t => t.LienPostingResponse).HasColumnName("HTTP_LIEN_POST_RES");
            builder.Property(t => t.LienMoficationRequest).HasColumnName("HTTP_LIEN_MODIFICATION_REQ");
            builder.Property(t => t.HttpLienModificationResponse).HasColumnName("HTTP_LIEN_MODIFICATION_RES");


            builder.ToTable("MOBILEAPP_SMART_LOAN_LOGS", "EBANKUSER");
        }
    }
}
