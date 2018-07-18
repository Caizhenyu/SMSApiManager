using AutoMapper;
using SMSApiManager.Models;
using SMSApiManager.Models.ManageViewModels;
using SMSApiManager.Models.MemberViewModles;
using SMSApiManager.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMSApiManager.Configurations
{
    /// <summary>
    /// 
    /// </summary>
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ApplicationUser, AccountDetailModel>();
            CreateMap<MemberView, Member>();
            CreateMap<CreateUserApiViewModel, UserApi>();
            CreateMap<UserApi, UserApiDetailViewModel>();
        }         

    }
}
