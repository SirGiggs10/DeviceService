using AutoMapper;
using DeviceService.Core.Dtos.AuditReport;
using DeviceService.Core.Dtos.AuditReportActivity;
using DeviceService.Core.Dtos.Auth;
using DeviceService.Core.Dtos.Device;
using DeviceService.Core.Dtos.DeviceOperation;
using DeviceService.Core.Dtos.DeviceType;
using DeviceService.Core.Dtos.RoleFunctionality;
using DeviceService.Core.Dtos.User;
using DeviceService.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceService.Core.Helpers.MapperProfiles
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserDetails>();
            CreateMap<UserDetails, UserLoginResponse>();
            CreateMap<User, UserToReturn>();
            CreateMap<User, UserWithUserTypeObjectResponse>();
            CreateMap<User, UserToReturnForLogin>();
            CreateMap<UserDetails, UserLoginResponseForLogin>();

            CreateMap<FunctionalityRequest, Functionality>();
            CreateMap<Functionality, FunctionalityResponse>();

            CreateMap<ProjectModuleRequest, ProjectModule>();
            CreateMap<ProjectModule, ProjectModuleResponse>();

            CreateMap<RoleRequest, Role>();
            CreateMap<Role, RoleResponse>();
            CreateMap<RoleResponse, Role>().ForMember(a2 => a2.CreatedAt, b2 => b2.Ignore());
            CreateMap<Role, RoleResponseForLogin>();
            CreateMap<UserRole, UserRoleResponseForLogin>();

            CreateMap<AuditReportActivityRequest, AuditReportActivity>();
            CreateMap<AuditReportActivity, AuditReportActivityResponse>();
            CreateMap<AuditReportActivityToUpdate, AuditReportActivity>();
            CreateMap<AuditReportRequest, AuditReport>();
            CreateMap<AuditReport, AuditReportResponse>();
            CreateMap<User, AuditReportUserResponse>();

            CreateMap<UserRole, UserRoleResponse>();
            CreateMap<UserRole, UserRoleToReturn>();
            CreateMap<FunctionalityRole, FunctionalityRoleResponse>();

            CreateMap<DeviceOperationRequest, DeviceOperation>();
            CreateMap<DeviceOperation, DeviceOperationResponse>();
            CreateMap<DeviceOperationToUpdate, DeviceOperation>();

            CreateMap<DeviceTypeRequest, DeviceType>();
            CreateMap<DeviceType, DeviceTypeResponse>();
            CreateMap<DeviceTypeToUpdate, DeviceType>();
            CreateMap<DeviceTypeOperationRequest, DeviceTypeOperation>();
            CreateMap<DeviceTypeOperation, DeviceTypeOperationResponse>();

            CreateMap<UserRequest, User>();
            CreateMap<User, UserResponse>();
            CreateMap<UserToUpdate, User>();

            CreateMap<DeviceRequest, Device>();
            CreateMap<Device, DeviceResponse>();
            CreateMap<DeviceToUpdate, Device>();
            CreateMap<StatusUpdate, Device>();
            CreateMap<TemperatureUpdate, Device>();
            CreateMap<UsageUpdate, Device>();
            CreateMap<Device, DevicePartialResponse>();
            CreateMap<Device, DeviceWithRelatedDevicesResponse>();
        }
    }
}
