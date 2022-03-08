using Ayuda_Help_Desk.Data;
using Ayuda_Help_Desk.Dtos;
using Ayuda_Help_Desk.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Ayuda_Help_Desk.Dtos.General;
using Ayuda_Help_Desk.Helpers;
using OfficeOpenXml;
using Aspose.Cells;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Identity;
using Ayuda_Help_Desk.Models;
using Microsoft.Extensions.Primitives;

namespace Ayuda_Help_Desk.Repositories
{
    public class GlobalRepository : IGlobalRepository
    {
        private readonly DataContext _dataContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _env;
        private readonly ICloudinaryRepository _cloudinaryRepository;
        private readonly UserManager<User> _userManager;


        public GlobalRepository(DataContext dataContext, ICloudinaryRepository cloudinaryRepository, UserManager<User> userManager, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = dataContext;
            _userManager = userManager;
            _env = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
            _cloudinaryRepository = cloudinaryRepository;
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

        public string GetMailBodyTemplate(string recipientFirstName, string recipientLastName, string link, string message1, string message2, string templateSrc)
        {
            string body = string.Empty;
            var originUrls = new StringValues();
            var originUrl = "";
            var originHeadersGotten = _httpContextAccessor.HttpContext.Request.Headers.TryGetValue("Origin", out originUrls);
            if (originHeadersGotten)
            {
                originUrl = originUrls.FirstOrDefault();
                if (string.IsNullOrWhiteSpace(originUrl))
                {
                    return null;
                }
            }
                //using streamreader for reading html template
                var webRoot = _env.WebRootPath;
            using (StreamReader reader = new StreamReader(Path.Combine(webRoot, templateSrc)))
            {
                body = reader.ReadToEnd();
            }
            if (templateSrc == "resolve-ticket.html")
            {
                body = body.Replace("{firstname}", recipientFirstName);
                body = body.Replace("{lastname}", recipientLastName);
                body = body.Replace("{link}", link);
                body = body.Replace("{message1}", message1);
                body = body.Replace("{message2}", message2);
                body = body.Replace("{LINKURL}", originUrl);
                body = body.Replace("{ayuda_logo}", Path.Combine(webRoot, "img/AyudaLogo.png"));
                body = body.Replace("{ayuda_logoLight}", Path.Combine(webRoot, "img/AyudaLogoLight.png"));
            }
            else if (templateSrc == "raise-ticket.html")
            {
                if (recipientFirstName != "" || recipientLastName != "")
                {
                    body = body.Replace("{firstname}", recipientFirstName);
                    body = body.Replace("{lastname}", recipientLastName);
                }
                body = body.Replace("{link}", link);
                body = body.Replace("{message1}", message1);
                body = body.Replace("{message2}", message2);
                body = body.Replace("{LINKURL}", originUrl);
                body = body.Replace("{ayuda_logo}", Path.Combine(webRoot, "img/AyudaLogo.png"));
                body = body.Replace("{ayuda_logoLight}", Path.Combine(webRoot, "img/AyudaLogoLight.png"));
            }
            else if(templateSrc == "bulkIndex.html")
            {
                if (recipientFirstName != "" || recipientLastName != "")
                {
                    body = body.Replace("{firstname}", recipientFirstName);
                    body = body.Replace("{lastname}", recipientLastName);
                }
                body = body.Replace("{link}", link);
                body = body.Replace("{message1}", message1);
                body = body.Replace("{message2}", message2);
                body = body.Replace("{LINKURL}", originUrl);
                body = body.Replace("{ayuda_logo}", Path.Combine(webRoot, "img/AyudaLogo.png"));
                body = body.Replace("{ayuda_logoLight}", Path.Combine(webRoot, "img/AyudaLogoLight.png"));
            }
            else if (templateSrc == "activation.html")
            {
                if (recipientFirstName != "" || recipientLastName != "")
                {
                    body = body.Replace("{firstname}", recipientFirstName);
                    body = body.Replace("{lastname}", recipientLastName);
                }
                body = body.Replace("{link}", link);
                body = body.Replace("{message1}", message1);
                body = body.Replace("{message2}", message2);
                body = body.Replace("{LINKURL}", originUrl);
                body = body.Replace("{ayuda_logo}", Path.Combine(webRoot, "img/AyudaLogo.png"));
                body = body.Replace("{ayuda_logoLight}", Path.Combine(webRoot, "img/AyudaLogoLight.png"));
            }
            else if (templateSrc == "resetpassword.html")
            {
                if (recipientFirstName != "" || recipientLastName != "")
                {
                    body = body.Replace("{firstname}", recipientFirstName);
                    body = body.Replace("{lastname}", recipientLastName);
                }
                body = body.Replace("{link}", link);
                body = body.Replace("{message1}", message1);
                body = body.Replace("{message2}", message2);
                body = body.Replace("{LINKURL}", originUrl);
                body = body.Replace("{ayuda_logo}", Path.Combine(webRoot, "img/AyudaLogo.png"));
                body = body.Replace("{ayuda_logoLight}", Path.Combine(webRoot, "img/AyudaLogoLight.png"));
            }
            else if (templateSrc == "resetpasswordcode.html")
            {
                if (recipientFirstName != "" || recipientLastName != "")
                {
                    body = body.Replace("{firstname}", recipientFirstName);
                    body = body.Replace("{lastname}", recipientLastName);
                }
                body = body.Replace("{message1}", message1);
                body = body.Replace("{message2}", message2);
                body = body.Replace("{LINKURL}", originUrl);
                body = body.Replace("{ayuda_logo}", Path.Combine(webRoot, "img/AyudaLogo.png"));
                body = body.Replace("{ayuda_logoLight}", Path.Combine(webRoot, "img/AyudaLogoLight.png"));
            }
            else if (templateSrc == "activationMobile.html")
            {
                if (recipientFirstName != "" || recipientLastName != "")
                {
                    body = body.Replace("{firstname}", recipientFirstName);
                    body = body.Replace("{lastname}", recipientLastName);
                }
                body = body.Replace("{link}", link);
                body = body.Replace("{message1}", message1);
                body = body.Replace("{message2}", message2);
                body = body.Replace("{LINKURL}", originUrl);
                body = body.Replace("{ayuda_logo}", Path.Combine(webRoot, "img/AyudaLogo.png"));
                body = body.Replace("{ayuda_logoLight}", Path.Combine(webRoot, "img/AyudaLogoLight.png"));
            }
            else
            {
                if (recipientFirstName != "" || recipientLastName != "")
                {
                    body = body.Replace("{firstname}", recipientFirstName);
                    body = body.Replace("{lastname}", recipientLastName);
                }
                body = body.Replace("{link}", link);
                body = body.Replace("{message1}", message1);
                body = body.Replace("{message2}", message2);
                body = body.Replace("{LINKURL}", originUrl);
                body = body.Replace("{ayuda_logo}", Path.Combine(webRoot, "img/AyudaLogo.png"));
                body = body.Replace("{ayuda_logoLight}", Path.Combine(webRoot, "img/AyudaLogoLight.png"));
            }
            return body;
        }


        public async Task<ReturnResponse> GetModuleFileLink(string moduleName)
        {
            if (moduleName == "Supervisor") {
                await ExportModuleFile("Supervisor");
                var moduleFile = await _dataContext.ModuleFile.Where(z => z.Deleted != Utils.Deleted && z.ModuleName == moduleName).FirstOrDefaultAsync();

                if (moduleFile == null)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotFound
                    };
                }

                return new ReturnResponse()
                {
                    StatusCode = Utils.Success,
                    ObjectValue = moduleFile
                };
            }
            else if (moduleName == "Role")
            {
                await ExportModuleFile("Role");
                var moduleFile = await _dataContext.ModuleFile
                    .Where(z => z.Deleted != Utils.Deleted && z.ModuleName == moduleName)
                    .FirstOrDefaultAsync();

                if (moduleFile == null)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotFound
                    };
                }

                return new ReturnResponse()
                {
                    StatusCode = Utils.Success,
                    ObjectValue = moduleFile
                };
            }
            else if (moduleName == "TicketPDF")
            {
                await ExportModuleFile("TicketPDF");
                var moduleFile = await _dataContext.ModuleFile
                    .Where(z => z.Deleted != Utils.Deleted && z.ModuleName == moduleName)
                    .FirstOrDefaultAsync();

                if (moduleFile == null)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotFound
                    };
                }

                return new ReturnResponse()
                {
                    StatusCode = Utils.Success,
                    ObjectValue = moduleFile
                };
            }
            else if (moduleName == "TicketExcel")
            {
                await ExportModuleFile("TicketExcel");
                var moduleFile = await _dataContext.ModuleFile
                    .Where(z => z.Deleted != Utils.Deleted && z.ModuleName == moduleName)
                    .FirstOrDefaultAsync();

                if (moduleFile == null)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotFound
                    };
                }

                return new ReturnResponse()
                {
                    StatusCode = Utils.Success,
                    ObjectValue = moduleFile
                };
            }
            else
            {
                var moduleFile = await _dataContext.ModuleFile
                    .Where(z => z.Deleted != Utils.Deleted && z.ModuleName == moduleName)
                    .FirstOrDefaultAsync();

                if (moduleFile == null)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotFound
                    };
                }

                return new ReturnResponse()
                {
                    StatusCode = Utils.Success,
                    ObjectValue = moduleFile
                };
            }
        }

        public async Task<ReturnResponse> ExportModuleFile(string moduleType)
        {
            var pdfFile = new FileInfo("file");
            if (moduleType == "MacroCategory")
            {
                var moduleDetails = _dataContext.MacroCategory.Where(b => b.Deleted != Utils.Deleted);
                string fileName = moduleType+".xlsx";
                var file = new FileInfo(fileName);
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                ExcelPackage excel = new ExcelPackage(file);
                var workSheet = excel.Workbook.Worksheets.Add("Sheet1");

                workSheet.Row(1).Height = 20;
                workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Row(1).Style.Font.Bold = true;

                workSheet.Cells[1, 1].Value = "CategoryId";
                workSheet.Cells[1, 2].Value = "CategoryName";

                int recordIndex = 2;
                foreach (var moduleDetail in moduleDetails)
                {
                    workSheet.Cells[recordIndex, 1].Value = moduleDetail.CategoryId;
                    workSheet.Cells[recordIndex, 2].Value = moduleDetail.CategoryName;
                    workSheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    recordIndex++;
                }

                workSheet.Column(1).AutoFit();
                workSheet.Column(2).AutoFit();

                excel.Workbook.Properties.Title = moduleType + " Table";
                excel.Save();

                // Instantiate the Workbook object with the Excel file
                Workbook workbook = new Workbook(moduleType + ".xlsx");

                // Save the document in PDF format
                workbook.Save(moduleType + ".pdf", SaveFormat.Pdf);
                pdfFile = new FileInfo(moduleType + ".pdf");

                excel.File.Refresh();
                excel.File.Delete();
            }
            else if (moduleType == "Department")
            {
                var moduleDetails = _dataContext.Department.Where(b => b.Deleted != Utils.Deleted);

                string fileName = moduleType+".xlsx";
                var file = new FileInfo(fileName);
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                ExcelPackage excel = new ExcelPackage(file);
                var workSheet = excel.Workbook.Worksheets.Add("Sheet1");

                workSheet.Row(1).Height = 20;
                workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Row(1).Style.Font.Bold = true;

                workSheet.Cells[1, 1].Value = "DepartmentId";
                workSheet.Cells[1, 2].Value = "DepartmentName";

                int recordIndex = 2;
                foreach (var moduleDetail in moduleDetails)
                {
                    workSheet.Cells[recordIndex, 1].Value = moduleDetail.DepartmentId;
                    workSheet.Cells[recordIndex, 2].Value = moduleDetail.DepartmentName;
                    workSheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    recordIndex++;
                }

                workSheet.Column(1).AutoFit();
                workSheet.Column(2).AutoFit();

                excel.Workbook.Properties.Title = moduleType+" Table";
                excel.Save();

                // Instantiate the Workbook object with the Excel file
                Workbook workbook = new Workbook(moduleType+".xlsx");

                // Save the document in PDF format
                workbook.Save(moduleType +".pdf", SaveFormat.Pdf);
                pdfFile = new FileInfo(moduleType +".pdf");

                excel.File.Refresh();
                excel.File.Delete();
            }
            else if (moduleType == "SubUnit")
            {
                var moduleDetails = _dataContext.SubUnit.Include(s => s.Department).Where(b => b.Deleted != Utils.Deleted);

                string fileName = moduleType + ".xlsx";
                var file = new FileInfo(fileName);
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                ExcelPackage excel = new ExcelPackage(file);
                var workSheet = excel.Workbook.Worksheets.Add("Sheet1");

                workSheet.Row(1).Height = 20;
                workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Row(1).Style.Font.Bold = true;

                workSheet.Cells[1, 1].Value = "SubUnitId";
                workSheet.Cells[1, 2].Value = "SubUnitName";
                workSheet.Cells[1, 3].Value = "DepartmentId";
                workSheet.Cells[1, 4].Value = "DepartmentName";

                int recordIndex = 2;
                foreach (var moduleDetail in moduleDetails)
                {
                    workSheet.Cells[recordIndex, 1].Value = moduleDetail.SubUnitId;
                    workSheet.Cells[recordIndex, 2].Value = moduleDetail.SubUnitName;
                    workSheet.Cells[recordIndex, 3].Value = moduleDetail.DepartmentId;
                    workSheet.Cells[recordIndex, 4].Value = moduleDetail.Department.DepartmentName;
                    workSheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    recordIndex++;
                }

                workSheet.Column(1).AutoFit();
                workSheet.Column(2).AutoFit();
                workSheet.Column(3).AutoFit();
                workSheet.Column(4).AutoFit();

                excel.Workbook.Properties.Title = moduleType + " Table";
                excel.Save();

                // Instantiate the Workbook object with the Excel file
                Workbook workbook = new Workbook(moduleType + ".xlsx");

                // Save the document in PDF format
                workbook.Save(moduleType + ".pdf", SaveFormat.Pdf);
                pdfFile = new FileInfo(moduleType + ".pdf");

                excel.File.Refresh();
                excel.File.Delete();
            }
            else if (moduleType == "Branch")
            {
                var moduleDetails = _dataContext.Branch.Include(s => s.Zone).Where(b => b.Deleted != Utils.Deleted);

                string fileName = moduleType + ".xlsx";
                var file = new FileInfo(fileName);
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                ExcelPackage excel = new ExcelPackage(file);
                var workSheet = excel.Workbook.Worksheets.Add("Sheet1");

                workSheet.Row(1).Height = 20;
                workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Row(1).Style.Font.Bold = true;

                workSheet.Cells[1, 1].Value = "BranchId";
                workSheet.Cells[1, 2].Value = "BranchName";
                workSheet.Cells[1, 3].Value = "BranchAddress";
                workSheet.Cells[1, 4].Value = "ZoneId";
                workSheet.Cells[1, 5].Value = "ZoneName";

                int recordIndex = 2;
                foreach (var moduleDetail in moduleDetails)
                {
                    workSheet.Cells[recordIndex, 1].Value = moduleDetail.BranchId;
                    workSheet.Cells[recordIndex, 2].Value = moduleDetail.BranchName;
                    workSheet.Cells[recordIndex, 3].Value = moduleDetail.BranchAddress;
                    workSheet.Cells[recordIndex, 4].Value = moduleDetail.Zone.ZoneId;
                    workSheet.Cells[recordIndex, 5].Value = moduleDetail.Zone.ZoneName;
                    workSheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    recordIndex++;
                }

                workSheet.Column(1).AutoFit();
                workSheet.Column(2).AutoFit();
                workSheet.Column(3).AutoFit();
                workSheet.Column(4).AutoFit();
                workSheet.Column(5).AutoFit();

                excel.Workbook.Properties.Title = moduleType + " Table";
                excel.Save();

                // Instantiate the Workbook object with the Excel file
                Workbook workbook = new Workbook(moduleType + ".xlsx");

                // Save the document in PDF format
                workbook.Save(moduleType + ".pdf", SaveFormat.Pdf);
                pdfFile = new FileInfo(moduleType + ".pdf");

                excel.File.Refresh();
                excel.File.Delete();
            }
            else if (moduleType == "Supervisor")
            {
                var moduleDetails = _dataContext.Staff.Take(10);

                string fileName = moduleType + ".xlsx";
                var file = new FileInfo(fileName);
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                ExcelPackage excel = new ExcelPackage(file);
                var workSheet = excel.Workbook.Worksheets.Add("Sheet1");

                workSheet.Row(1).Height = 20;
                workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Row(1).Style.Font.Bold = true;

                workSheet.Cells[1, 1].Value = "StaffId";
                workSheet.Cells[1, 2].Value = "StaffName";

                int recordIndex = 2;
                foreach (var moduleDetail in moduleDetails)
                {
                    workSheet.Cells[recordIndex, 1].Value = moduleDetail.StaffId;
                    workSheet.Cells[recordIndex, 2].Value = moduleDetail.FullName;
                    workSheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    recordIndex++;
                }

                workSheet.Column(1).AutoFit();
                workSheet.Column(2).AutoFit();

                excel.Workbook.Properties.Title = moduleType + " Table";
                excel.Save();

                // Instantiate the Workbook object with the Excel file
                Workbook workbook = new Workbook(moduleType + ".xlsx");

                // Save the document in PDF format
                workbook.Save(moduleType + ".pdf", SaveFormat.Pdf);
                pdfFile = new FileInfo(moduleType + ".pdf");

                excel.File.Refresh();
                excel.File.Delete();
            }
            else if (moduleType == "Role")
            {
                var moduleDetails = _dataContext.Roles;

                string fileName = moduleType + ".xlsx";
                var file = new FileInfo(fileName);
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                ExcelPackage excel = new ExcelPackage(file);
                var workSheet = excel.Workbook.Worksheets.Add("Sheet1");

                workSheet.Row(1).Height = 20;
                workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Row(1).Style.Font.Bold = true;

                workSheet.Cells[1, 1].Value = "RoleId";
                workSheet.Cells[1, 2].Value = "RoleName";

                int recordIndex = 2;
                foreach (var moduleDetail in moduleDetails)
                {
                    workSheet.Cells[recordIndex, 1].Value = moduleDetail.Id;
                    workSheet.Cells[recordIndex, 2].Value = moduleDetail.RoleName;
                    workSheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    recordIndex++;
                }

                workSheet.Column(1).AutoFit();
                workSheet.Column(2).AutoFit();

                excel.Workbook.Properties.Title = moduleType + " Table";
                excel.Save();

                // Instantiate the Workbook object with the Excel file
                Workbook workbook = new Workbook(moduleType + ".xlsx");

                // Save the document in PDF format
                workbook.Save(moduleType + ".pdf", SaveFormat.Pdf);
                pdfFile = new FileInfo(moduleType + ".pdf");

                excel.File.Refresh();
                excel.File.Delete();
            }
            else if (moduleType == "TicketPDF")
            {
                var moduleDetails = new List<Ticket>();
                var userType = _httpContextAccessor.HttpContext.User
                    .Claims
                    .FirstOrDefault(x => x.Type == Utils.ClaimType_UserType);
                var userTypeId = _httpContextAccessor.HttpContext.User
                    .Claims
                    .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
                if ((userType == null) || (userTypeId == null))
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = Utils.StatusMessageNotFound
                    };
                }

                var userTypeVal = Convert.ToInt32(userType.Value);
                var userTypeIdVal = Convert.ToInt32(userTypeId.Value);
                if (userTypeVal == Utils.Staff)
                {
                    var userStaffDetails = await _userManager.Users
                        .Where(a => (a.UserTypeId == userTypeIdVal) && (a.UserType == userTypeVal) && (!a.Deleted))
                        .Include(b => b.Staff)
                        .Include(c => c.UserRoles)
                        .ThenInclude(d => d.Role)
                        .FirstOrDefaultAsync();
                    try
                    {
                        if ((userStaffDetails == null) || 
                            (userStaffDetails.Staff == null) || 
                            (userStaffDetails.UserRoles == null))
                        {
                            return new ReturnResponse()
                            {
                                StatusCode = Utils.NotFound,
                                StatusMessage = Utils.StatusMessageNotFound
                            };
                        }
                    }
                    catch (Exception)
                    {
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.NotFound,
                            StatusMessage = Utils.StatusMessageNotFound
                        };
                    }

                    var userStaffRolesSupportLevels = userStaffDetails.UserRoles
                        .Select(a => a.Role)
                        .Select(b => b.SupportLevelId)
                        .ToList();
                    moduleDetails = await _dataContext.Ticket
                        .Include(a => a.TicketAllowedUsers)
                        .Where(b => b.TicketAllowedUsers.Any(c => (c.DepartmentId == userStaffDetails.Staff.DepartmentId) && ((c.SubUnitId == userStaffDetails.Staff.SubUnitId)
                        || (c.SubUnitId == Utils.AllSubUnit)) && (userStaffRolesSupportLevels.Any(d => d == c.SupportLevelId))/*(userStaffRolesSupportLevels.Contains(c.SupportLevelId))*/))
                        .Include(e => e.AgentStaff).Include(g => g.TicketCategory)
                        .Include(h => h.Customer).Include(j => j.ServiceLevel)
                        .ToListAsync();

                }

                string fileName = moduleType + ".xlsx";
                var file = new FileInfo(fileName);
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                ExcelPackage excel = new ExcelPackage(file);
                var workSheet = excel.Workbook.Worksheets.Add("Sheet1");

                workSheet.Row(1).Height = 20;
                workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Row(1).Style.Font.Bold = true;

                workSheet.Cells[1, 1].Value = "TicketId";
                workSheet.Cells[1, 2].Value = "Subject";
                workSheet.Cells[1, 3].Value = "Customer";
                workSheet.Cells[1, 4].Value = "Date Requested";
                workSheet.Cells[1, 5].Value = "Priority Level";
                workSheet.Cells[1, 6].Value = "Agent";
                workSheet.Cells[1, 7].Value = "Status";

                int recordIndex = 2;
                foreach (var moduleDetail in moduleDetails)
                {
                    workSheet.Cells[recordIndex, 1].Value = moduleDetail.TicketId;
                    workSheet.Cells[recordIndex, 2].Value = moduleDetail.Subject;
                    workSheet.Cells[recordIndex, 3].Value = moduleDetail.Customer.FullName;
                    workSheet.Cells[recordIndex, 4].Value = moduleDetail.CreatedAt;
                    workSheet.Cells[recordIndex, 5].Value = moduleDetail.ServiceLevel.PriorityName;
                    if (moduleDetail.AgentStaffId != 0)
                    {
                        workSheet.Cells[recordIndex, 6].Value = moduleDetail.AgentStaff.FullName;
                    }
                    workSheet.Cells[recordIndex, 7].Value = moduleDetail.TicketStatusId;
                    workSheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    recordIndex++;
                }

                workSheet.Column(1).AutoFit();
                workSheet.Column(2).AutoFit();
                workSheet.Column(3).AutoFit();
                workSheet.Column(4).AutoFit();
                workSheet.Column(5).AutoFit();
                workSheet.Column(6).AutoFit();
                workSheet.Column(7).AutoFit();

                excel.Workbook.Properties.Title = moduleType + " Table";
                excel.Save();

                // Instantiate the Workbook object with the Excel file
                Workbook workbook = new Workbook(moduleType + ".xlsx");

                // Save the document in PDF format
                workbook.Save(moduleType + ".pdf", SaveFormat.Pdf);
                pdfFile = new FileInfo(moduleType + ".pdf");

                excel.File.Refresh();
                excel.File.Delete();
            }
            else if (moduleType == "TicketExcel")
            {
                var moduleDetails = new List<Ticket>();
                var userType = _httpContextAccessor.HttpContext.User
                    .Claims
                    .FirstOrDefault(x => x.Type == Utils.ClaimType_UserType);
                var userTypeId = _httpContextAccessor.HttpContext.User
                    .Claims
                    .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
                if ((userType == null) || (userTypeId == null))
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = Utils.StatusMessageNotFound
                    };
                }

                var userTypeVal = Convert.ToInt32(userType.Value);
                var userTypeIdVal = Convert.ToInt32(userTypeId.Value);
                if (userTypeVal == Utils.Staff)
                {
                    var userStaffDetails = await _userManager.Users
                        .Where(a => (a.UserTypeId == userTypeIdVal) && (a.UserType == userTypeVal) && (!a.Deleted))
                        .Include(b => b.Staff)
                        .Include(c => c.UserRoles)
                        .ThenInclude(d => d.Role)
                        .FirstOrDefaultAsync();
                    try
                    {
                        if ((userStaffDetails == null) || 
                            (userStaffDetails.Staff == null) || 
                            (userStaffDetails.UserRoles == null))
                        {
                            return new ReturnResponse()
                            {
                                StatusCode = Utils.NotFound,
                                StatusMessage = Utils.StatusMessageNotFound
                            };
                        }
                    }
                    catch (Exception)
                    {
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.NotFound,
                            StatusMessage = Utils.StatusMessageNotFound
                        };
                    }

                    var userStaffRolesSupportLevels = userStaffDetails.UserRoles
                        .Select(a => a.Role)
                        .Select(b => b.SupportLevelId)
                        .ToList();
                    moduleDetails = await _dataContext.Ticket
                        .Include(a => a.TicketAllowedUsers)
                        .Where(b => b.TicketAllowedUsers.Any(c => (c.DepartmentId == userStaffDetails.Staff.DepartmentId) && ((c.SubUnitId == userStaffDetails.Staff.SubUnitId)
                        || (c.SubUnitId == Utils.AllSubUnit)) && (userStaffRolesSupportLevels.Any(d => d == c.SupportLevelId))/*(userStaffRolesSupportLevels.Contains(c.SupportLevelId))*/))
                        .Include(e => e.AgentStaff).Include(g => g.TicketCategory)
                        .Include(h => h.Customer).Include(j => j.ServiceLevel)
                        .ToListAsync();

                }

                string fileName = moduleType + ".xlsx";
                var file = new FileInfo(fileName);
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                ExcelPackage excel = new ExcelPackage(file);
                var workSheet = excel.Workbook.Worksheets.Add("Sheet1");

                workSheet.Row(1).Height = 20;
                workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Row(1).Style.Font.Bold = true;

                workSheet.Cells[1, 1].Value = "TicketId";
                workSheet.Cells[1, 2].Value = "Subject";
                workSheet.Cells[1, 3].Value = "Customer";
                workSheet.Cells[1, 4].Value = "Date Requested";
                workSheet.Cells[1, 5].Value = "Priority Level";
                workSheet.Cells[1, 6].Value = "Agent";
                workSheet.Cells[1, 7].Value = "Status";

                int recordIndex = 2;
                foreach (var moduleDetail in moduleDetails)
                {
                    workSheet.Cells[recordIndex, 1].Value = moduleDetail.TicketId;
                    workSheet.Cells[recordIndex, 2].Value = moduleDetail.Subject;
                    workSheet.Cells[recordIndex, 3].Value = moduleDetail.Customer.FullName;
                    workSheet.Cells[recordIndex, 4].Value = moduleDetail.CreatedAt;
                    workSheet.Cells[recordIndex, 5].Value = moduleDetail.ServiceLevel.PriorityName;
                    if (moduleDetail.AgentStaffId != 0)
                    {
                        workSheet.Cells[recordIndex, 6].Value = moduleDetail.AgentStaff.FullName;
                    }
                    workSheet.Cells[recordIndex, 7].Value = moduleDetail.TicketStatusId;
                    workSheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    recordIndex++;
                }

                workSheet.Column(1).AutoFit();
                workSheet.Column(2).AutoFit();
                workSheet.Column(3).AutoFit();
                workSheet.Column(4).AutoFit();
                workSheet.Column(5).AutoFit();
                workSheet.Column(6).AutoFit();
                workSheet.Column(7).AutoFit();

                excel.Workbook.Properties.Title = moduleType + " Table";
                excel.Save();

                // Instantiate the Workbook object with the Excel file
                Workbook workbook = new Workbook(moduleType + ".xlsx");

                // Save the document in PDF format
                workbook.Save(moduleType + ".csv", SaveFormat.CSV);
                pdfFile = new FileInfo(moduleType + ".csv");

                excel.File.Refresh();
                excel.File.Delete();
            }

            var cloudinaryResult = _cloudinaryRepository.UploadExcelFileToCloudinary(pdfFile);
            if (cloudinaryResult.StatusCode != Utils.Success)
            {
                if (cloudinaryResult.StatusCode != Utils.ObjectNull)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.CloudinaryFileUploadError
                    };
                }
            }
            else
            {
                var cloudinaryUploadResult = (RawUploadResult)cloudinaryResult.ObjectValue;
                var moduleFile = await _dataContext.ModuleFile
                    .Where(a => a.ModuleName == moduleType)
                    .FirstOrDefaultAsync();

                if (moduleFile == null)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.ObjectNull
                    };
                }
                moduleFile.FileLink = cloudinaryUploadResult.Uri.ToString();
                moduleFile.PublicId = cloudinaryUploadResult.PublicId;
                _dataContext.Entry(moduleFile).State = EntityState.Modified;
                await _dataContext.SaveChangesAsync();
            }

            pdfFile.Delete();

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = pdfFile
            };

        }

    }
}
