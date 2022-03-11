using DeviceService.Core.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceService.Core.Interfaces.UnitOfWorks
{
    public interface IDeviceDb_UnitOfWork
    {
        IDeviceRepository DeviceRepository { get; }
        IDeviceTypeRepository DeviceTypeRepository { get; }
        IDeviceOperationRepository DeviceOperationRepository { get; }
        IUserRepository UserRepository { get; }
        IAuthRepository AuthRepository { get; }

    }
}
