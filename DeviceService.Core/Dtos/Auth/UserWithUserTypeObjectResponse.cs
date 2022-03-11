﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceService.Core.Dtos.Auth
{
    public class UserWithUserTypeObjectResponse
    {
        public int Id { get; set; }
        public int UserType { get; set; }
        public int UserTypeId { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTimeOffset? LastLoginDateTime { get; set; }
        public DateTimeOffset? SecondToLastLoginDateTime { get; set; }
    }
}
