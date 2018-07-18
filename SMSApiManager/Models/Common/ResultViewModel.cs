using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SMSApiManager.Models.ViewModels
{
    public class ResultViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public object Errors { get; set; }

        public ResultViewModel() { }
        public ResultViewModel(string name, bool succeeded)
        {
            Name = name;
            Succeeded = succeeded;
        }
        public ResultViewModel(string name, bool succeeded, string message)
        {
            Name = name;
            Succeeded = succeeded;
            Message = message;
        }
        public ResultViewModel(string name, bool succeeded, object errors)
        {
            Name = name;
            Succeeded = succeeded;
            Errors = errors;
        }
        public ResultViewModel(string name, bool succeeded, string message, object errors)
        {
            Name = name;
            Succeeded = succeeded;
            Message = message;
            Errors = errors;
        }
        public ResultViewModel(int id, bool succeeded)
        {
            Id = id;
            Succeeded = succeeded;
        }
        public ResultViewModel(int id, bool succeeded, string message)
        {
            Id = id;
            Succeeded = succeeded;
            Message = message;
        }
        public ResultViewModel(int id, bool succeeded, object errors)
        {
            Id = id;
            Succeeded = succeeded;
            Errors = errors;
        }
        public ResultViewModel(int id, bool succeeded, string message, object errors)
        {
            Id = id;
            Succeeded = succeeded;
            Message = message;
            Errors = errors;
        }
    }
}
