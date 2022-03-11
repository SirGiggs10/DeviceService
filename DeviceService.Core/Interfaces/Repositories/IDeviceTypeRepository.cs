using DeviceService.Core.Dtos.Device;
using DeviceService.Core.Dtos.DeviceType;
using DeviceService.Core.Dtos.Global;
using DeviceService.Core.Helpers.Pagination;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DeviceService.Core.Interfaces.Repositories
{
    public interface IDeviceTypeRepository
    {
        Task<ReturnResponse<DeviceTypeResponse>> CreateDeviceType(DeviceTypeRequest deviceTypeRequest);
        Task<ReturnResponse<DeviceTypeResponse>> GetDeviceType(int deviceTypeId);
        Task<ReturnResponse<List<DeviceTypeResponse>>> GetDeviceTypes(UserParams userParam);
        Task<ReturnResponse<DeviceTypeResponse>> UpdateDeviceType(int deviceTypeId, DeviceTypeToUpdate deviceTypeToUpdate);
        Task<ReturnResponse<List<DeviceTypeResponse>>> DeleteDeviceType(List<int> deviceTypeIds);
    }
}
