
using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Backend.APIs.Hubs;
using Microsoft.Identity.Web;
using Microsoft.Extensions.Logging;
using Backend.GameInfrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Logging;
using Backend.APIs.AuthorizationPolicies;
using Backend.GameInfrastructure.DataModels;

using Newtonsoft.Json;

namespace Backend
{
    public class Startup
    {
        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            Configuration = configuration;
            this.Logger = logger;
        }

        public IConfiguration Configuration { get; }
        public ILogger<Startup> Logger { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
#if DEBUG
            IdentityModelEventSource.ShowPII = true;
#endif

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                 .AddMicrosoftWebApi(options =>
                 { 
                     Configuration.Bind("AzureAdB2C", options);

                     options.TokenValidationParameters.NameClaimType = "name";
                 },
                options => { Configuration.Bind("AzureAdB2C", options); });

            services.AddAuthorization(options =>
            {
                // Create policy to check for the scope 'read'
                options.AddPolicy("LobbyManagement",
                    policy => policy.Requirements.Add(new ScopesRequirement("LobbyManagement")));
            });

            // The following line enables Application Insights telemetry collection.
            services.AddApplicationInsightsTelemetry();
            services.AddSingleton(typeof(GameManager));
            services.AddSingleton(typeof(InMemoryConfiguration));

            services.AddSignalR(hubOptions =>
            {
                hubOptions.EnableDetailedErrors = true;
                hubOptions.KeepAliveInterval = TimeSpan.FromSeconds(20);

                // Keeping this extra long because clients don't handle disconnects well currently and pause in background.
                hubOptions.ClientTimeoutInterval = TimeSpan.FromMinutes(5);
            });

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.WithOrigins("https://scrawlbrawl.b2clogin.com/", Configuration.GetValue<string>("FrontendUrl")).AllowAnyHeader().AllowAnyMethod();
                    });
            });

            services.AddControllers().AddNewtonsoftJson();
            services.AddHostedService<GameNotifier>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseCookiePolicy();

            //app.UseLetsEncryptFolder(env);
            //app.UseStaticFiles();
            //app.UseSpaStaticFiles();

            app.UseRouting();
            app.UseCors();

            if (!env.IsDevelopment())
            {
                app.UseAuthentication();
                app.UseAuthorization();
            }

            // SPAs, Sockets, and Endpoints oh my!
            app.UseWebSockets(new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
                ReceiveBufferSize = 4 * 1024
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
                endpoints.MapHub<UnityHub>("/signalr", options =>
                {
                    options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling; // you may also need this
                });
            });

            /*app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:50402");
                }
            });*/
        }
    }
}
