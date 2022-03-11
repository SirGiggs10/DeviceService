using DeviceService.Core.Interfaces;
using DeviceService.Core.Interfaces.Repositories;
using DeviceService.Core.Interfaces.UnitOfWorks;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceService.Core.Data.UnitOfWorks
{
    public class DeviceDb_UnitOfWork : IDeviceDb_UnitOfWork, IDisposable
    {
        public IDeviceRepository DeviceRepository { get; }

        public IDeviceTypeRepository DeviceTypeRepository { get; }

        public IDeviceOperationRepository DeviceOperationRepository { get; }

        public IUserRepository UserRepository { get; }

        public IAuthRepository AuthRepository { get; }

        public void Dispose()
        {
            
        }
    }
}
