using Microsoft.AspNetCore.Http;

namespace Ayuda_Help_Desk.Dtos.Staff
{
    public class StaffPictureRequest
    {
        public IFormFile StaffProfilePicture { get; set; }
    }
}
