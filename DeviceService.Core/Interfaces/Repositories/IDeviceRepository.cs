using DeviceService.Core.Dtos.Device;
using DeviceService.Core.Dtos.Global;
using DeviceService.Core.Helpers.Pagination;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static DeviceService.Core.Helpers.Common.Utils;

namespace DeviceService.Core.Interfaces.Repositories
{
    public interface IDeviceRepository
    {
        Task<ReturnResponse<DeviceResponse>> CreateDevice(DeviceRequest deviceRequest);
        Task<ReturnResponse<DeviceResponse>> GetDevice(int deviceId);
        Task<ReturnResponse<List<DeviceResponse>>> GetDevices(UserParams userParam);
        Task<ReturnResponse<List<DeviceResponse>>> GetDevicesForUser(UserParams userParam);
        Task<ReturnResponse<List<DevicePartialResponse>>> GetDevicesByStatus(UserParams userParam, DeviceStatus deviceStatus);
        Task<ReturnResponse<List<DevicePartialResponse>>> GetDevicesByStatusForUser(UserParams userParam, DeviceStatus deviceStatus);
        Task<ReturnResponse<List<DevicePartialResponse>>> GetDevicesByDeviceType(UserParams userParam, int deviceTypeId);
        Task<ReturnResponse<List<DevicePartialResponse>>> GetDevicesByDeviceTypeForUser(UserParams userParam, int deviceTypeId);
        Task<ReturnResponse<DeviceWithRelatedDevicesResponse>> GetDeviceWithRelatedDevices(int deviceId);
        Task<ReturnResponse<DevicePartialResponse>> UpdateDeviceName(int deviceId, DeviceToUpdate deviceToUpdate);
        Task<ReturnResponse<DevicePartialResponse>> UpdateDeviceStatus(int deviceId, StatusUpdate statusUpdate);
        Task<ReturnResponse<DevicePartialResponse>> UpdateDeviceTemperature(int deviceId, TemperatureUpdate temperatureUpdate);
        Task<ReturnResponse<DevicePartialResponse>> UpdateDeviceUsageHours(int deviceId, UsageUpdate usageUpdate);
        Task<ReturnResponse<List<DevicePartialResponse>>> DeleteDevice(List<int> deviceIds);
        Task<ReturnResponse<List<DeviceResponse>>> SearchUserDevice(DeviceSearchRequest deviceSearchRequest, UserParams userParam);
    }
}
