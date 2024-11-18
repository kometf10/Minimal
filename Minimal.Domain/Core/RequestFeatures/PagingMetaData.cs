using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minimal.Domain.Core.RequestFeatures
{
    public class PagingMetaData
    {
        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }

        public int PageSize { get; set; } = 10;

        public int TotalCount { get; set; }

        public bool HasPrevios => CurrentPage > 1;

        public bool HasNext => CurrentPage < TotalPages;
    }
}
