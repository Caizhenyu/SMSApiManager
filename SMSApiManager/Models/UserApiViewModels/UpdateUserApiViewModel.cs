using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMSApiManager.Models.ViewModels
{
    public class UpdateUserApiViewModel
    {
        public string ApiName { get; set; }
        public ApiStatus Status { get; set; }
        public string Remark { get; set; }
    }
}
