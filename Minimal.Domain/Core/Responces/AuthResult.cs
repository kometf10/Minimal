using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minimal.Domain.Core.Responces
{
    public class AuthResult
    {
        public IEnumerable<string>? Errors { get; set; }

        public bool Successed { get; set; }

        public string? Token { get; set; }

        public string? RefreshToken { get; set; }

    }
}
