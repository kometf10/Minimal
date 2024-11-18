using Microsoft.Extensions.FileProviders;
using Minmal.API.Middlewares;

namespace Minmal.API.Extensions
{
    public static class AppConfig
    {
        public static void Configure(this WebApplication app, IConfiguration configuration)
        {
            if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
                app.ConfigureStaticFiles();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseCors("AllowAllPolicy");

            app.UseMiddleware<JwtMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static void ConfigureStaticFiles(this WebApplication app)
        {
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"StaticFiles")),
                RequestPath = new PathString("/StaticFiles")
            });
        }
    }
}
