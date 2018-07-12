using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SMSApiManager.Models
{
    public class Record
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecordId { get; set; }

        [ForeignKey("User")]
        [Column(Order = 1)]
        [Required]
        public string OwnerId { get; set; }

        [Column(Order = 2)]
        [Required]
        public int ApiId { get; set; }
        public int SendCount { get; set; }
        public DateTime SendTime { get; set; }

        public Api Api { get; set; }
        public ApplicationUser User { get; set; }
    }
}
