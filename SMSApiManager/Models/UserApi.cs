using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SMSApiManager.Models
{
    public enum ApiStatus
    {
        NoUse = 0,
        Normal = 1,
        Exception = 2
    }
    public class UserApi : IResource
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("User")]
        [Column(Order = 1)]
        [Required]
        public string OwnerId { get; set; }

        [Required]
        public string ApiNo { get; set; }

        public string ApiName { get; set; }

        public ApiStatus Status { get; set; } = ApiStatus.Normal;

        [Required]
        public string Address { get; set; }

        public string Remark { get; set; }

        public ApplicationUser User { get; set; }
    }
}
