using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Ayuda_Help_Desk.Models
{
    public class FunctionalitySupportLevel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public int SupportLevelId { get; set; }
        public string SupportLevelName { get; set; }
        public int FunctionalityId { get; set; }
        public string FunctionalityName { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }

        public virtual Functionality Functionality { get; set; }
        public virtual SupportLevel SupportLevel { get; set; }
    }
}
