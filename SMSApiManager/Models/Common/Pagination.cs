using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMSApiManager.Models.Common
{
    public class Pagination
    {
        private int _pageSize = 10;
        private int _maxPageSize = 100;
        public int PageIndex { get; set; } = 0;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > _maxPageSize ? _maxPageSize : value;
        }
        public string OrderBy { get; set; }
        public string SearchValue { get; set; }
    }
}
