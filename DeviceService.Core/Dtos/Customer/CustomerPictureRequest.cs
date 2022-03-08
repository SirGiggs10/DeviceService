using Microsoft.AspNetCore.Http;

namespace Ayuda_Help_Desk.Dtos.Customer
{
    public class CustomerPictureRequest
    {
        public IFormFile CustomerProfilePicture { get; set; }
    }
}
