using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minimal.Domain.Core.Responces
{
    public class ValidationResult
    {
        public string? Field { get; set; }
        public List<string> Errors { get; set; } = new List<string>();

    }
}
