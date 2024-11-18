using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minimal.Domain.Core.Administration
{
    public class RefreshTokenRequest 
    {
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }

    }
}
