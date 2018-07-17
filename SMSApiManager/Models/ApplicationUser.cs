using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SMSApiManager.Models
{
    public enum UserStatus
    {
        NoUse = 0,
        Normal = 1
    }

    public class ApplicationUser : IdentityUser, IResource
    {
        public string UserNo { get; set; }

        [ForeignKey("User")]
        public string OwnerId { get; set; }
        public string Name { get; set; }
        public DateTime Birthday { get; set; }
        public string Address { get; set; }

        public Level Level { get; set; } = Level.Admin;
        public UserStatus Status { get; set; } = UserStatus.Normal;
        public string PermissionList { get; set; }

        public ApplicationUser User { get; set; }
    }
}
