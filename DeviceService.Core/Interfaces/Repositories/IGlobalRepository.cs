﻿using DeviceService.Core.Dtos.Auth;
using DeviceService.Core.Dtos.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceService.Core.Interfaces.Repositories
{
    public interface IGlobalRepository
    {
        public bool Add<TEntity>(TEntity entity) where TEntity : class;
        public Task<bool> Add<TEntity>(List<TEntity> entities) where TEntity : class;
        public bool Update<TEntity>(TEntity entity) where TEntity : class;
        public bool Update<TEntity>(List<TEntity> entities) where TEntity : class;
        public bool Delete<TEntity>(TEntity entity) where TEntity : class;
        public bool Delete<TEntity>(List<TEntity> entities) where TEntity : class;
        public Task<bool?> SaveAll();
        public Task<TEntity> Get<TEntity>(int id) where TEntity : class;
        public Task<List<TEntity>> Get<TEntity>() where TEntity : class;
        public LoggedInUserInfo GetUserInformation();
    }
}
