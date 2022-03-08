using Ayuda_Help_Desk.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ayuda_Help_Desk.Dtos.Staff
{
    public class StaffForRegisterDto
    {
        public List<StaffRequest> Staff { get; set; }
    }
}
