using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SMSApiManager.Models
{
    public enum MemberStatus
    {
        NoUse = 0,
        Normal = 1
    }

    public enum ContactStatus
    {
        Submitted,
        Approved,
        Rejected
    }

    public class Member
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MemberId { get; set; }

        // user ID from AspNetUser table
        [ForeignKey("User")]
        [Required]
        public string OwnerId { get; set; }

        public string MemberNo { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }

        [DataType(DataType.Date)]
        public DateTime Birthday { get; set; }

        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [DefaultValue(1)]
        public MemberStatus Status { get; set; } = MemberStatus.Normal;

        public ContactStatus ContactStatus { get; set; }

        public ApplicationUser User { get; set; }
    }
}
