using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SMSApiManager.Models.ViewModels
{
    public class CreateUserApiViewModel
    {
        [Required]
        public string ApiNo { get; set; }

        public string ApiName { get; set; }

        //public ApiStatus Status { get; set; } = ApiStatus.Normal;

        [Required]
        public string Address { get; set; }

        public string Remark { get; set; }
    }
}
