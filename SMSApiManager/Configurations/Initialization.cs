using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SMSApiManager.Data;
using SMSApiManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMSApiManager.Configurations
{
    public static class Initialization
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                string password = "1234567890Ab";
                var user = new ApplicationUser();
                user.Email = "2521195169@qq.com";
                user.UserName = "2521195169@qq.com"; 
                user.Name = "蔡振宇";
                user.UserNo = "System001";
                user.PhoneNumber = "18721206605";
                user.Level = Level.System;

                var uid = await EnsureUser(serviceProvider, user, password);
                await EnsureRole(serviceProvider, uid, user.Level.ToString());
            }
        }

        private static async Task<string> EnsureUser(IServiceProvider serviceProvider,
                                            ApplicationUser user, string password)
        {
            var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();

            var newUser = await userManager.FindByEmailAsync(user.Email);
            if (newUser == null)
            {
                await userManager.CreateAsync(user, password);
            }

            return newUser.Id;
        }

        private static async Task<IdentityResult> EnsureRole(IServiceProvider serviceProvider,
                                                              string uid, string role)
        {
            IdentityResult IR = null;
            var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();

            if (!await roleManager.RoleExistsAsync(role))
            {
                IR = await roleManager.CreateAsync(new IdentityRole(role));
            }

            var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();

            var user = await userManager.FindByIdAsync(uid);

            IR = await userManager.AddToRoleAsync(user, role);

            return IR;
        }
    }
}
