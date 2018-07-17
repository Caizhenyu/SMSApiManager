using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMSApiManager.Models.UserApiViewModels
{
    public class CreateUserApiViewModel
    {
        public string ApiNo { get; set; }

        public string ApiName { get; set; }

        //public ApiStatus Status { get; set; } = ApiStatus.Normal;

        public string Address { get; set; }

        public string Remark { get; set; }
    }
}
