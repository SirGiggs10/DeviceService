using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ayuda_Help_Desk.Models
{
    public class Customer
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int CustomerId { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public int CustomerTypeId { get; set; }
        public string CustomerProfilePictureUrl { get; set; }
        public string CustomerProfilePicturePublicId { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }

        public virtual CustomerType CustomerType { get; set; }
        public virtual User User { get; set; }
        public virtual List<Ticket> Tickets { get; set; }
        public virtual List<SplittedTicketParent> SplittedTicketParents { get; set; }
        public virtual List<MergedTicketParent> MergedTicketParents { get; set; }
        //public virtual List<WhatsAppMessage> WhatsAppMessage { get; set; }
    }
}
