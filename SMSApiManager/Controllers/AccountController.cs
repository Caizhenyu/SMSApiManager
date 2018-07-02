using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthorizePolicy.JWT;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMSApiManager.Data;
using SMSApiManager.Models;
using SMSApiManager.Models.AccountViewModels;
using SMSApiManager.Services;

namespace SMSApiManager.Controllers
{
    [Authorize("Permission")]
    [EnableCors("MyDomain")]
    [Produces("application/json")]
    [Route("api/[controller]/[action]")]
    public class AccountController : Controller
    {
        /// <summary>
        /// 自定义策略参数
        /// </summary>
        private readonly PermissionRequirement _requirement;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _applicationDbContext;

        public AccountController(
            PermissionRequirement requirement,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IEmailSender emailSender,
            ILogger<AccountController> logger,
            ApplicationDbContext applicationDbContext)
        {
            _requirement = requirement;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _logger = logger;
            _applicationDbContext = applicationDbContext;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);

                    var roles = await _userManager.GetRolesAsync(user);
                    //var roles = from role in _applicationDbContext.Roles
                    //            join userRole in _applicationDbContext.UserRoles
                    //            on role.Id equals userRole.RoleId
                    //            select role.Name;

                    if (roles.Count() == 0)
                    {
                        switch (user.Level)
                        {
                            case Level.Admin:
                                if (!await _roleManager.RoleExistsAsync(Level.Admin.ToString()))
                                {
                                    await _roleManager.CreateAsync(new IdentityRole(Level.Admin.ToString()));
                                }
                                await _userManager.AddToRoleAsync(user, Level.Admin.ToString());
                                break;

                            case Level.SuperAdmin:
                                if (!await _roleManager.RoleExistsAsync(Level.SuperAdmin.ToString()))
                                {
                                    await _roleManager.CreateAsync(new IdentityRole(Level.SuperAdmin.ToString()));
                                }
                                await _userManager.AddToRoleAsync(user, Level.SuperAdmin.ToString());
                                break;

                            case Level.System:
                                if (!await _roleManager.RoleExistsAsync(Level.System.ToString()))
                                {
                                    await _roleManager.CreateAsync(new IdentityRole(Level.System.ToString()));
                                }
                                await _userManager.AddToRoleAsync(user, Level.System.ToString());
                                break;
                        }

                        roles = await _userManager.GetRolesAsync(user);
                    }

                    
                    //如果是基于用户的授权策略，这里要添加用户;如果是基于角色的授权策略，这里要添加角色
                    var claims = new Claim[] { new Claim(ClaimTypes.Name, user.UserName), new Claim(ClaimTypes.Role, roles.FirstOrDefault()), new Claim(ClaimTypes.Expiration, DateTime.Now.AddSeconds(_requirement.Expiration.TotalSeconds).ToString()) };
                    //用户标识
                    var identity = new ClaimsIdentity(JwtBearerDefaults.AuthenticationScheme);
                    identity.AddClaims(claims);

                    var token = JwtToken.BuildJwtToken(claims, _requirement);
                    return new JsonResult(token);

                }
                else if (result.IsLockedOut)
                {
                    //_logger.LogWarning("User account locked out.");
                    return RedirectToAction(nameof(Lockout));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Ok(model);
                }
            }

            // If we got this far, something failed, redisplay form
            return new JsonResult(new
            {
                Status = false,
                Message = "认证失败"
            });
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            //ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserNo = model.UserNo, Name = model.Name, UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
                    await _emailSender.SendEmailConfirmationAsync(model.Email, callbackUrl);

                    //await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation("User created a new account with password.");
                    return new JsonResult(new
                    {
                        IsSuccess = true,
                        Message = "注册成功"
                    });
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return Ok(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Denied()
        {
            return new JsonResult(new
            {
                Status = false,
                Message = "你无权限访问"
            });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Lockout()
        {
            return Ok();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return Ok();
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{userId}'.");
            }
            var result = await _userManager.ConfirmEmailAsync(user, code);
            return Ok(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return Ok();
            }
        }

        #endregion
    }
}
