using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minimal.Domain.Core.Administration
{
    public class LoginRequest
    {
        public string? Email { get; set; }

        public string? UserName { get; set; }

        [PasswordPropertyText]
        public string? Password { get; set; }
    }
}
