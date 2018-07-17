using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMSApiManager.Models
{
    public interface IResource
    {
        string OwnerId { get; set; }
    }
}
