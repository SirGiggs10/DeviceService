using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ayuda_Help_Desk.Dtos.Auth
{
    public class UserToReturnForLogin
    {
        public int Id { get; set; }
        public int UserType { get; set; } // specifies the type of user like customer or staff
        public int UserTypeId { get; set; } // specifies the id of the user on his type table...like staff table
        public bool EmailConfirmed { get; set; }
        public DateTimeOffset? LastLoginDateTime { get; set; }
        public DateTimeOffset? SecondToLastLoginDateTime { get; set; }
        public List<UserRoleResponseForLogin> UserRoles { get; set; }
    }
}
