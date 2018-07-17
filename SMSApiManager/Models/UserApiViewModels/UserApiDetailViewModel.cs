using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMSApiManager.Models.UserApiViewModels
{
    public class UserApiDetailViewModel
    {
        public string ApiNo { get; set; }

        public string ApiName { get; set; }

        public ApiStatus Status { get; set; }

        public string Address { get; set; }

        public string Remark { get; set; }
    }
}
