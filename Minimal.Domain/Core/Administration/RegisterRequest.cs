using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minimal.Domain.Core.Administration
{
    public class RegisterRequest
    {
        public string? Email { get; set; }

        public string? UserName { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Password { get; set; }


        public string? ConfirmPassword { get; set; }

    }
}
