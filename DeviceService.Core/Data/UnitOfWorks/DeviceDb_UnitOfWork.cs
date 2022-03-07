using DeviceService.Core.Interfaces.UnitOfWorks;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceService.Core.Data.UnitOfWorks
{
    public class DeviceDb_UnitOfWork : IDeviceDb_UnitOfWork
    {
        public IDeviceRepository DeviceRepository { get; }
    }
}
