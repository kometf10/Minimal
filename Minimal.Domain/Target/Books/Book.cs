using Minimal.Domain.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minimal.Domain.Target.Books
{
    public class Book : BaseEntity
    {
        public string? Title { get; set; }

        public string? Author { get; set; }

    }
}
