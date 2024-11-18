using Microsoft.AspNetCore.Authorization;

namespace Minmal.API.Extensions
{
    public static class PolicyAuthorizationExtention
    {
        public static IServiceCollection AddPolicyBasedAuthorization(this IServiceCollection services)
        {

            services.AddAuthorizationCore(options => {
                options.AddPolicy("administration-roles", policy => policy.RequireAssertion(context => AdminOrCan(context, "administration-roles")));
                options.AddPolicy("administration-users", policy => policy.RequireAssertion(context => AdminOrCan(context, "administration-users")));
                options.AddPolicy("app-settings-control", policy => policy.RequireAssertion(context => AdminOrCan(context, "app-settings-control")));
                options.AddPolicy("audeting-access", policy => policy.RequireAssertion(context => AdminOrCan(context, "audeting-access")));
                options.AddPolicy("error-log-access", policy => policy.RequireAssertion(context => AdminOrCan(context, "error-log-access")));
                options.AddPolicy("push-notification-control", policy => policy.RequireAssertion(context => AdminOrCan(context, "push-notification-control")));
                options.AddPolicy("reporting-control", policy => policy.RequireAssertion(context => AdminOrCan(context, "reporting-control")));
                options.AddPolicy("indexes-control", policy => policy.RequireAssertion(context => AdminOrCan(context, "indexes-control")));
                options.AddPolicy("requests-control", policy => policy.RequireAssertion(context => AdminOrCan(context, "requests-control")));
                options.AddPolicy("data-access-control", policy => policy.RequireAssertion(context => AdminOrCan(context, "data-access-control")));

            });

            return services;
        }

        public static bool AdminOrCan(AuthorizationHandlerContext context, string claim)
        {
            bool isAdmin = context.User.IsInRole("Admin");
            bool can = context.User.HasClaim("Permission", claim);

            return isAdmin || can;
        }
        public static bool AdminOrCan(AuthorizationHandlerContext context, List<string> claims)
        {
            bool isAdmin = context.User.IsInRole("Admin");

            foreach (string c in claims)
                if (context.User.HasClaim("Permission", c))
                    return true;
            return isAdmin;
        }

        public static bool AdminOrCanAll(AuthorizationHandlerContext context, List<string> claims)
        {
            bool isAdmin = context.User.IsInRole("Admin");

            bool canAll = true;
            foreach (string c in claims)
                if (!context.User.HasClaim("Permission", c))
                    canAll = false;

            return isAdmin || canAll;
        }

        public static bool Can(AuthorizationHandlerContext context, string claim)
        {
            return context.User.HasClaim("Permission", claim);
        }

        public static bool Can(AuthorizationHandlerContext context, List<string> claims)
        {

            foreach (string c in claims)
                if (context.User.HasClaim("Permission", c))
                    return true;
            return false;

        }

        public static bool CanAll(AuthorizationHandlerContext context, List<string> claims)
        {
            foreach (string c in claims)
                if (!context.User.HasClaim("Permission", c))
                    return false;
            return true;
        }
    }
}
