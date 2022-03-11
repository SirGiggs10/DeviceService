using DeviceService.Core.Dtos.DeviceOperation;
using DeviceService.Core.Dtos.Global;
using DeviceService.Core.Helpers.Pagination;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DeviceService.Core.Interfaces.Repositories
{
    public interface IDeviceOperationRepository
    {
        Task<ReturnResponse<DeviceOperationResponse>> CreateDeviceOperation(DeviceOperationRequest deviceOperationRequest);
        Task<ReturnResponse<DeviceOperationResponse>> GetDeviceOperation(int deviceOperationId);
        Task<ReturnResponse<List<DeviceOperationResponse>>> GetDeviceOperations(UserParams userParam);
        Task<ReturnResponse<DeviceOperationResponse>> UpdateDeviceOperation(int deviceOperationId, DeviceOperationToUpdate deviceOperationToUpdate);
        Task<ReturnResponse<List<DeviceOperationResponse>>> DeleteDeviceOperation(List<int> deviceOperationIds);
    }
}
