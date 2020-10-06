
using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RoystonGame.Web.Hubs;
using Microsoft.Identity.Web;
using RoystonGame.Web.Helpers.Extensions;
using Microsoft.Extensions.Logging;
using RoystonGame.TV;
using Microsoft.ApplicationInsights.Extensibility.EventCounterCollector;
using RoystonGame.Web.Helpers.Telemetry;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Logging;
using RoystonGame.Web.AuthorizationPolicies;

namespace RoystonGame
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
            // The following code shows several customizations done to EventCounterCollectionModule.
            services.ConfigureTelemetryModule<EventCounterCollectionModule>(
                (module, o) =>
                {
                    // This removes all default counters.
                    //module.Counters.Clear();

                    // This adds a user defined counter "Users" from EventSource named "Application"
                    module.Counters.Add(new EventCounterCollectionRequest("Application", "LobbyStart"));
                    module.Counters.Add(new EventCounterCollectionRequest("Application", "LobbyEnd"));
                    module.Counters.Add(new EventCounterCollectionRequest("Application", "GameStart"));
                    module.Counters.Add(new EventCounterCollectionRequest("Application", "SignalRConnect"));
                    module.Counters.Add(new EventCounterCollectionRequest("Application", "SignalRDisconnect"));
                    module.Counters.Add(new EventCounterCollectionRequest("Application", "GameError"));
                }
            );
            services.AddSingleton(typeof(RgEventSource));
            services.AddSingleton(typeof(GameManager));

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
                        builder.WithOrigins("https://scrawlbrawl.b2clogin.com/");
                    });
            });

            services.AddControllers().AddNewtonsoftJson();

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });
            services.AddHostedService<GameNotifier>();
            services.AddApplicationInsightsTelemetry();
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
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseRouting();
            app.UseCors();

#if !DEBUG
            app.UseAuthentication();
            app.UseAuthorization();
#endif

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

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:50402");
                }
            });
        }
    }
}
