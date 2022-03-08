using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ayuda_Help_Desk.Dtos
{
    public class ResetPasswordSendMailRequest
    {
        public string EmailAddress { get; set; }
    }

    public class ResetPasswordRequest
    {
        public string NewPassword { get; set; }
        public string PasswordResetLinkToken { get; set; }
    }

    public class ResetPasswordCodeRequest
    {
        public string EmailAddress { get; set; }
        public string NewPassword { get; set; }
        public string Otp { get; set; }
    }
}
