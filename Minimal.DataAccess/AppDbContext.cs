using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Minimal.Domain.Core.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minimal.DataAccess
{
    public partial class AppDbContext : IdentityDbContext<User>
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly string? UserDataAccessKey;
        public readonly DbContextOptions<AppDbContext> Options;

        public AppDbContext(DbContextOptions<AppDbContext> options,
                    IServiceProvider serviceProvider,
                    IHostEnvironment environment) : base(options)
        {
            httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            //httpContextAccessor = this.GetInfrastructure().GetRequiredService<IHttpContextAccessor>();
            UserDataAccessKey = GetUserDataAccessKey();
            Options = options;
        }

        public IHttpContextAccessor HttpContextAccessor => httpContextAccessor;


    }
}
