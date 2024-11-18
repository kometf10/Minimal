using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minimal.Domain.Core.RequestFeatures
{
    public class RequestParameters
    {
        const int MaxPageSize = 50;

        public int PageNumber { get; set; } = 1;

        private int _pagesize = 10;

        public int PageSize
        {
            get
            {
                return _pagesize;
            }
            set
            {
                _pagesize = (value > MaxPageSize) ? MaxPageSize : value;
            }
        }

        public IEnumerable<FilterParam>? FilterParams { get; set; }

        public string OrderColumn { get; set; } = "Id";
        public string OrderType { get; set; } = "DESC";

        public string Gather { get; set; } = "AND";

        public string QuickFilter { get; set; } = string.Empty;

        public void ClearFilters()
            => FilterParams = new List<FilterParam>();

        public void AddFilter(FilterParam filter)
        {
            var filters = FilterParams?.ToList() ?? new List<FilterParam>();
            filters.Add(filter);
            FilterParams = filters;
        }

        public void AddFilters(List<FilterParam> filterToAdd)
        {
            var filters = FilterParams?.ToList() ?? new List<FilterParam>();
            filters.AddRange(filterToAdd);
            FilterParams = filters;
        }
    }
}
