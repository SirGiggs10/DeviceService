using Ayuda_Help_Desk.Dtos;
using Ayuda_Help_Desk.Dtos.General;
using Ayuda_Help_Desk.Dtos.Staff;
using Ayuda_Help_Desk.Helpers;
using Ayuda_Help_Desk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ayuda_Help_Desk.Interfaces
{
    public interface IStaffRepository
    {
        public Task<ReturnResponse> RegisterStaff(StaffForRegisterDto staff);
        public Task<ReturnResponse> SetStaffPasswordAndProfilePicture(StaffPasswordRequest staffPasswordRequest);
        public Task<ReturnResponse> SetProfilePicture(StaffPictureRequest staffPasswordRequest);
        public Task<ReturnResponse> GetStaff(UserParams userParams);
        public Task<ReturnResponse> GetAllStaffUnpaginated();
        public Task<ReturnResponse> GetStaff(int id);
        public Task<ReturnResponse> AdminUpdateStaff(StaffUpdateRequest staff, int id);
        public Task<ReturnResponse> GetMockStaffRequests();
        public Task<ReturnResponse> UpdateStaff(StaffUpdateRequest staff);
        public Task<ReturnResponse> DeleteStaff(List<StaffResponse> staff);
        public Task<ReturnResponse> GetSupportLevelStaffCount();
        public Task<ReturnResponse> SearchStaff(string searchParams, UserParams userParams);
    }
}
