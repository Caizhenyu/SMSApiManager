using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMSApiManager.Models
{
    public enum Level
    {
        Admin = 1,
        SuperAdmin = 2,
        System = 3
    }
    public enum UserStatus
    {
        NoUse = 0,
        Normal = 1
    }

    public class ApplicationUser : IdentityUser
    {
        public string UserNo { get; set; }

        public string Name { get; set; }
        public DateTime Birthday { get; set; }
        public string Address { get; set; }

        public Level Level { get; set; } = Level.Admin;
        public UserStatus Status { get; set; }
        public string PermissionList { get; set; }
    }
}
