using Microsoft.AspNetCore.Localization;
using Minimal.DataAccess;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Minimal.Services.Core.Transactions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Minimal.Domain.Core.Administration;
using Minimal.Domain.Core.Settings;
using System.Text;
using Minimal.Services.Core.Auth;
using Minimal.Domain.Core.Helpers;
using Minmal.API.ActionFilters;
using Microsoft.AspNetCore.Mvc;
using FluentValidation.AspNetCore;

namespace Minmal.API.Extensions
{
    public static class ServicesInjections
    {
        public static void AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddITSControllers();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddEFCore(configuration);
            services.AddAuth(configuration);
            services.AddITSCors();
            services.AddITSLocalization();

            services.AddScoped<ITransactionsService, TransactionsService>();
            services.AddScoped<IAuthService, AuthService>();
        }

        private static void AddITSControllers(this IServiceCollection services)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            services.AddControllers(options =>
            {
                //use custom validation filter attrubute 
                options.Filters.Add(typeof(ValidateModelStateAttribute));
            })
            //.AddDataAnnotationsLocalization(options =>
            //{
            //    options.DataAnnotationLocalizerProvider = (type, factory) =>
            //    {
            //        var assemblyName = new AssemblyName(typeof(CommonResources).GetTypeInfo().Assembly.FullName!);
            //        return factory.Create(nameof(CommonResources), assemblyName.Name!);
            //    };
            //})
            .AddNewtonsoftJson(options => {
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                options.SerializerSettings.Converters.Add(new DateOnlyJsonConverter());
                options.SerializerSettings.Converters.Add(new TimeOnlyJsonConverter());
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                //suppress dafault model state validation filter
                options.SuppressModelStateInvalidFilter = true;
            })
            .AddFluentValidation(fv =>
            {
                fv.DisableDataAnnotationsValidation = true;
                fv.RegisterValidatorsFromAssemblyContaining<LoginRequest>();
            });
#pragma warning restore CS0618 // Type or member is obsolete

        }

        private static void AddITSCors(this IServiceCollection services)
        {
            services.AddCors(o => o.AddPolicy("AllowAllPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders("X-Pagination");
            }));
        }

        private static void AddAuth(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredUniqueChars = 0;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            var jwtSettings = new JwtSettings();
            configuration.Bind(key: nameof(JwtSettings), jwtSettings);
            services.AddSingleton(jwtSettings);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key: Encoding.ASCII.GetBytes(jwtSettings.Secret!)),
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    RequireExpirationTime = true,
                    ValidateLifetime = true
                };
            });

            services.AddAuthorization();

            services.AddPolicyBasedAuthorization();
        }

        private static void AddITSLocalization(this IServiceCollection services)
        {
            services.AddLocalization();

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCusltures = new List<CultureInfo>
                {
                    new CultureInfo("en-US"),
                    new CultureInfo("ar-AU")
                };

                options.SupportedCultures = supportedCusltures;
                options.SupportedUICultures = supportedCusltures;
                options.DefaultRequestCulture = new RequestCulture(culture: "en-US", uiCulture: "en-US");
            });
        }


        private static void AddEFCore(this IServiceCollection services, IConfiguration configuration)
        {
            //services.AddEntityFrameworkSqlServer(); //This Will Inject IMemoryCache With Size Limit 10024 (Removed When Manulay adding Memmory Cache with No Size Limit)
            services.AddDbContext<AppDbContext>((serviceProvider, options) =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"), mig => mig.MigrationsAssembly("Minimal.DataAccess"));
                options.UseApplicationServiceProvider(serviceProvider);
                //options.UseInternalServiceProvider(serviceProvider);
            }, ServiceLifetime.Scoped);
            
        }

    }
}
