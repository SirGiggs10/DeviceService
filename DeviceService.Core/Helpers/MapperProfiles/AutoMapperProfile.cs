using AutoMapper;
using CBN_eNaira.Core.Dtos.CBNENairaService;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceService.Core.Helpers.MapperProfiles
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            //USERTYPE SERVICE USER AUTHORIZE WITHDRAWAL
            CreateMap<AuthorizeUserTypeWithdrawalRequest, CustomerAuthorizeWithdrawalRequest>()
                .ForMember(dest => dest.CustomerAuthToken, action => { action.MapFrom(source => source.UserAuthToken); })
                .ForPath(dest => dest.WithdrawalEnableDetail.enableWithdrawal, action => { action.MapFrom(source => source.AuthorizeWithdrawal); });
            CreateMap<ReturnResponse<CustomerAuthorizeWithdrawalResponse>, ReturnResponse<AuthorizeUserTypeWithdrawalResponse>>();
            CreateMap<CustomerAuthorizeWithdrawalResponse, AuthorizeUserTypeWithdrawalResponse>()
                .ForMember(dest => dest.State, action => { action.MapFrom(source => source.enableWithdrawal.state); })
                .ForMember(dest => dest.ExpiryDate, action => { action.MapFrom(source => source.enableWithdrawal.expiry); });
            CreateMap<AuthorizeUserTypeWithdrawalRequest, MerchantAuthorizeWithdrawalRequest>()
                .ForMember(dest => dest.MerchantAuthToken, action => { action.MapFrom(source => source.UserAuthToken); })
                .ForPath(dest => dest.WithdrawalEnableDetail.enableWithdrawal, action => { action.MapFrom(source => source.AuthorizeWithdrawal); });
            CreateMap<ReturnResponse<MerchantAuthorizeWithdrawalResponse>, ReturnResponse<AuthorizeUserTypeWithdrawalResponse>>();
            CreateMap<MerchantAuthorizeWithdrawalResponse, AuthorizeUserTypeWithdrawalResponse>()
                .ForMember(dest => dest.State, action => { action.MapFrom(source => source.enableWithdrawal.state); })
                .ForMember(dest => dest.ExpiryDate, action => { action.MapFrom(source => source.enableWithdrawal.expiry); });
        }
    }
}
