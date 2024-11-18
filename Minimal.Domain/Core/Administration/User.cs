using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minimal.Domain.Core.Administration
{
    public class User : IdentityUser
    {
        public string? FullName { get; set; }

        public string? RefreshTokn { get; set; }

        public DateTime RefreshTokenExpiryTime { get; set; }

        public string? DataAccessKey { get; set; }

        public bool FirstUse { get; set; } = true;

        public DateTime? LastPasswordChange { get; set; }

        public bool ConfirmedAccount { get; set; }

    }
}
