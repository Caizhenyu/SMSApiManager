using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMSApiManager.Extensions
{
    public static class ModelStateExtensions
    {
        public static SerializableError GetErrors(this ModelStateDictionary modelState)
        {
            return new SerializableError(modelState);
        }
    }
}
