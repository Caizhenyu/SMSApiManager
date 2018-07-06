using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMSApiManager.Models.ManageViewModels
{
    public class AccountDetailModel
    {
        public string UserNo { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime Birthday { get; set; }
        public string Address { get; set; }

        public string Level { get; set; }
        
    }
}
