using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using SMSApiManager.Data;
using SMSApiManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SMSApiManager.Authorization
{
    public class ContactSuperAdminAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, IResource>
    {
        //private readonly ApplicationDbContext _applicationDbContext;
        //public ContactSuperAdminAuthorizationHandler(ApplicationDbContext applicationDbContext)
        //{
        //    _applicationDbContext = applicationDbContext;
        //}

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, IResource resource)
        {
            if (context.User == null)
            {
                return Task.CompletedTask;
            }

            //SuperAdmin 拥有其域内 Admin 的权限
            //var userId = context.User.FindFirst(ClaimTypes.Sid).Value;
            //var admin = _applicationDbContext.Users.FirstOrDefault(u => u.OwnerId == userId);
            //if(admin != null)
            //{
            //    context.Succeed(requirement);
            //}
            //Administrators can do anything.
            if (context.User.IsInRole(Level.SuperAdmin.ToString()))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }

    }
}
