using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using SMSApiManager.Data;
using SMSApiManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AuthorizePolicy.JWT
{
    /// <summary>
    /// 权限授权Handler
    /// </summary>
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private IAuthenticationSchemeProvider _schemes { get; set; }

        private ApplicationDbContext _applicationDbContext { get; set; }

        private IList<Permission> _permissions { get; set; }

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="schemes"></param>
        public PermissionHandler(IAuthenticationSchemeProvider schemes,
            ApplicationDbContext applicationDbContext)
        {
            _schemes = schemes;
            _applicationDbContext = applicationDbContext;

            _permissions = _applicationDbContext.Permission.ToList();

        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
          
            //从AuthorizationHandlerContext转成HttpContext，以便取出表求信息
            var httpContext = (context.Resource as Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext).HttpContext;
            //请求Url
            var questUrl = httpContext.Request.Path.Value.ToLower();
            //判断请求是否停止
            var handlers = httpContext.RequestServices.GetRequiredService<IAuthenticationHandlerProvider>();
            foreach (var scheme in await _schemes.GetRequestHandlerSchemesAsync())
            {
                var handler = await handlers.GetHandlerAsync(httpContext, scheme.Name) as IAuthenticationRequestHandler;
                if (handler != null && await handler.HandleRequestAsync())
                {
                    context.Fail();
                    return;
                }
            }
            //判断请求是否拥有凭据，即有没有登录
            var defaultAuthenticate = await _schemes.GetDefaultAuthenticateSchemeAsync();
            if (defaultAuthenticate != null)
            {
                var result = await httpContext.AuthenticateAsync(defaultAuthenticate.Name);
                //result?.Principal不为空即登录成功
                if (result?.Principal != null)
                {

                    httpContext.User = result.Principal;
                    var name = httpContext.User.Claims.SingleOrDefault(s => s.Type == requirement.ClaimType).Value;
                    
                    if(name == Level.System.ToString())
                    {

                    }
                    //else if(name == Level.SuperAdmin.ToString())
                    //{

                    //}
                    //权限中是否存在请求的url
                    else if (_permissions.GroupBy(g => g.Url).Where(w => w.Key.ToLower() == questUrl).Count() > 0)
                    {                        
                        //验证权限
                        if (_permissions.Where(w => w.Name == name && w.Url.ToLower() == questUrl).Count() <= 0)
                        {
                            //无权限跳转到拒绝页面
                            httpContext.Response.Redirect(requirement.DeniedAction);
                        }
                    }
                    //判断过期时间
                    if (DateTime.Parse(httpContext.User.Claims.SingleOrDefault(s => s.Type == ClaimTypes.Expiration).Value) >= DateTime.Now)
                    {
                        context.Succeed(requirement);
                    }
                    else
                    {
                        context.Fail();
                    }
                    return;
                }
            }
            //判断没有登录时，是否访问登录的url,并且是Post请求，并且是form表单提交类型，否则为失败
            if (!questUrl.Equals(requirement.LoginPath.ToLower(), StringComparison.Ordinal) && (!httpContext.Request.Method.Equals("POST")
               || !httpContext.Request.HasFormContentType))
            {
                context.Fail();
                return;
            }
            context.Succeed(requirement);
        }
    }
}
