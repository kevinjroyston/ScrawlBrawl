using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using System.IO;

namespace RoystonGame.Web.Helpers.Extensions
{
    public static class IApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds a file provider for '.well-known' folder that allows lets encrypt to verify domain requests.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        /// <remarks>See SO answer: http://stackoverflow.com/a/38406699 </remarks>
        public static IApplicationBuilder UseLetsEncryptFolder(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            var wellKnownDirectory = new DirectoryInfo(Path.Combine(env.ContentRootPath, ".well-known"));
            if (!wellKnownDirectory.Exists)
            {
                wellKnownDirectory.Create();
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                ServeUnknownFileTypes = true,
                FileProvider = new PhysicalFileProvider(wellKnownDirectory.FullName),
                RequestPath = new PathString("/.well-known"),
            });
            return app;
        }
    }
}
