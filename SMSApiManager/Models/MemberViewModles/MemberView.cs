using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SMSApiManager.Models.MemberViewModles
{
    public class MemberView
    {
        
        public string MemberNo { get; set; }
        [Required]
        public string Name { get; set; }
        public string Address { get; set; }
        [Required]
        public string PhoneNumber { get; set; }

        [DataType(DataType.Date)]
        public DateTime Birthday { get; set; }

        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        public MemberStatus Status { get; set; } = MemberStatus.Normal;

        public ContactStatus ContactStatus { get; set; }
    }
}
