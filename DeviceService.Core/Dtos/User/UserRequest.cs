using DeviceService.Core.Helpers.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DeviceService.Core.Dtos.User
{
    public class UserRequest
    {
        [Encrypted]
        public string EmailAddress { get; set; }
        [Encrypted]
        public string Password { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        [MaxLength(20, ErrorMessage = "PhoneNumber Length cannot be More than 20 Characters")]
        public string PhoneNumber { get; set; }
        [Required]
        public string RoleToAssignToUser { get; set; }
    }
}
