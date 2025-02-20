﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minimal.Domain.Core.RequestFeatures
{
    public class PagedList<T> : List<T>
    {
        public PagingMetaData PagingData { get; set; }

        public PagedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            PagingData = new PagingMetaData()
            {
                TotalCount = count,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling(count / (double)pageSize)
            };

            AddRange(items);
        }

        public static PagedList<T> ToPagedList(IEnumerable<T> source, int pageNumber, int pageSize)
        {
            var count = source.Count();
            var items = source
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize).ToList();
            return new PagedList<T>(items, count, pageNumber, pageSize);
        }

        public static PagedList<T> ToPagedList(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = source.Count();

            var pagedQueryable = source
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            var items = pagedQueryable.ToList();

            return new PagedList<T>(items, count, pageNumber, pageSize);
        }
    }
}
