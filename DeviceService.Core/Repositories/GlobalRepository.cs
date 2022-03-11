﻿using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Primitives;
using DeviceService.Core.Data.DataContext;
using DeviceService.Core.Dtos.Global;
using DeviceService.Core.Interfaces.Repositories;
using DeviceService.Core.Entities;
using DeviceService.Core.Dtos.Auth;

namespace DeviceService.Core.Repositories
{
    public class GlobalRepository : IGlobalRepository
    {
        private readonly DeviceContext _dataContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<User> _userManager;


        public GlobalRepository(DeviceContext dataContext, UserManager<User> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _dataContext = dataContext;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public bool Add<TEntity>(TEntity entity) where TEntity : class
        {
            try
            {
                _dataContext.Add(entity);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> Add<TEntity>(List<TEntity> entities) where TEntity : class
        {
            try
            {
                await _dataContext.AddRangeAsync(entities.AsEnumerable());
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public bool Delete<TEntity>(TEntity entity) where TEntity : class
        {
            try
            {
                _dataContext.Remove(entity);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool Delete<TEntity>(List<TEntity> entities) where TEntity : class
        {
            try
            {
                _dataContext.RemoveRange(entities.AsEnumerable());
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool?> SaveAll()
        {
            try
            {
                int saveStatus = await _dataContext.SaveChangesAsync();
                if (saveStatus > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public bool Update<TEntity>(TEntity entity) where TEntity : class
        {
            try
            {
                _dataContext.Entry(entity).State = EntityState.Modified;
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public bool Update<TEntity>(List<TEntity> entities) where TEntity : class
        {
            try
            {
                _dataContext.UpdateRange(entities.AsEnumerable());
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }       
        }

        public async Task<TEntity> Get<TEntity>(int id) where TEntity : class
        {
            var entity = await _dataContext.FindAsync<TEntity>(id);
            return entity;
        }

        public async Task<List<TEntity>> Get<TEntity>() where TEntity : class
        {
            var entity = await _dataContext.Set<TEntity>().ToListAsync();
            return entity;
        }

        public LoggedInUserInfo GetUserInformation()
        {
            var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name).Value;
            var userTypeId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value);
            var userRoles = _httpContextAccessor.HttpContext.User.Claims.Where(n => n.Type == ClaimTypes.Role).ToList();
            if (userRoles == null)
            {
                return new LoggedInUserInfo()
                {
                    UserId = userId,
                    UserTypeId = userTypeId
                };
            }

            return new LoggedInUserInfo()
            {
                UserId = userId,
                UserTypeId = userTypeId,
                Roles = userRoles
            };
        }
    }
}
