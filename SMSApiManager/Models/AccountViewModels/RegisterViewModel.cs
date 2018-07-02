using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SMSApiManager.Models.AccountViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "编号 *")]
        public string UserNo { get; set; }

        [Required]
        [Display(Name = "姓名 *")]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email *")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "密码 *")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "确认密码 *")]
        [Compare("Password", ErrorMessage = "两次密码不一致")]
        public string ConfirmPassword { get; set; }
    }
}
