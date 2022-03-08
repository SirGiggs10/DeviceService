using Ayuda_Help_Desk.Data;
using Ayuda_Help_Desk.Dtos;
using Ayuda_Help_Desk.Dtos.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ayuda_Help_Desk.Interfaces
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
        public string GetMailBodyTemplate(string recipientFirstName, string recipientLastName, string link, string message1, string message2, string templateSrc);
        public Task<ReturnResponse> GetModuleFileLink(string moduleName);
        public Task<ReturnResponse> ExportModuleFile(string moduleType);
    }
}
