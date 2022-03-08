using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ayuda_Help_Desk.Models
{
    public class Staff
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int StaffId { get; set; }
        public int UserId { get; set; }
        public string CompanyId { get; set; }
        public string FullName { get; set; }
        public int DepartmentId { get; set; }
        public int SubUnitId { get; set; }
        public int SupervisorId { get; set; }
        public int BranchId { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string ResidentialAddress { get; set; }
        public DateTimeOffset? DateOfBirth { get; set; }
        public string StaffProfilePictureUrl { get; set; }
        public string StaffProfilePicturePublicId { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        [NotMapped]
        public virtual User Supervisor { get; set; }

        public virtual Department Department { get; set; }
        public virtual SubUnit SubUnit { get; set; }
        public virtual User User { get; set; }
        public virtual Branch Branch { get; set; }
        public virtual List<Ticket> Tickets { get; set; }
        public virtual List<TicketResponseChat> TicketResponseChats { get; set; }
        public virtual List<SplittedTicketParent> SplittedTicketParents { get; set; }
        public virtual List<MergedTicketParent> MergedTicketParents { get; set; }
        public virtual List<OutgoingMailDetail> OutgoingMailDetails { get; set; }
    }
}
