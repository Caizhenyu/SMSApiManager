using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SMSApiManager.Models
{
    public enum ApiStatus
    {
        NoUse = 0,
        Normal = 1
    }
    public class Api
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ApiId { get; set; }

        public string ApiName { get; set; }

        public ApiStatus Status { get; set; } = ApiStatus.Normal;

        public string Address { get; set; }
    }
}
