// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Bcc.Members.Identity.Domain.Quickstart.Users;
using Microsoft.AspNetCore.HttpOverrides;
using IdentityServer4.Quickstart.UI;
using System.Threading.Tasks;

namespace Bcc.Members.Identity.Domain
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public IConfiguration _config { get; }

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            _config = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {              

            //services.AddHostedService<BackgroundTelegramBotService>();
            services.AddControllersWithViews();

            // configures IIS out-of-proc settings (see https://github.com/aspnet/AspNetCore/issues/14882)
            services.Configure<IISOptions>(iis =>
            {
                iis.AuthenticationDisplayName = "Windows";
                iis.AutomaticAuthentication = false;
            });

            // configures IIS in-proc settings
            services.Configure<IISServerOptions>(iis =>
            {
                iis.AuthenticationDisplayName = "Windows";
                iis.AutomaticAuthentication = false;
            });

            services.AddDbContext<ApplicationDbContext>(config =>
            {
                // for in memory database
                config.UseInMemoryDatabase("MemoryBaseDataBase");
            });

            // AddIdentity :-  Registers the services
            services.AddIdentity<AppUser, IdentityRole>(config =>
            {
                // User defined password policy settings.
                config.Password.RequiredLength = 4;
                config.Password.RequireDigit = false;
                config.Password.RequireNonAlphanumeric = false;
                config.Password.RequireUppercase = false;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            var builder = services.AddIdentityServer(options =>
            {
                options.Csp.AddDeprecatedHeader = false;
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;                
            })
            .AddRedirectUriValidator<CustomRedirectUriValidator>()
            .AddConfigurationStore(options =>
             {
                 options.ConfigureDbContext = builder => builder.UseInMemoryDatabase("MemoryBaseDataBase");
             })
            .AddOperationalStore(options =>
            {
                options.ConfigureDbContext = builder => builder.UseInMemoryDatabase("MemoryBaseDataBase");
                options.EnableTokenCleanup = true;
            })
            .AddAspNetIdentity<AppUser>()
            .AddProfileService<ProfileService>();


            // in-memory, code config
            builder.AddInMemoryIdentityResources(Config.Ids);
            builder.AddInMemoryApiResources(Config.Apis);
            builder.AddInMemoryClients(Config.Clients);
           
            // Generate 6000 random users
            var serviceCollection = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            var sc = services.BuildServiceProvider();
            var userManager = sc.GetService<UserManager<AppUser>>();
            var testUser = new TestUsers(userManager);
            Task.Run(() => testUser.AddRandomUsersToTheUserStore());
            

            // not recommended for production - you need to store your key material somewhere secure
            builder.AddDeveloperSigningCredential();
            services.AddApplicationInsightsTelemetry(_config["ApplicationInsights_InstrumentationKey"]);
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // This part is only applicable when the app is run behind a reverse proxy like Kubernetes or Google Cloud Run,
                // it basically has the effect that the discovery document reference https in it urls and not http
                // see this github ticket for more context https://github.com/IdentityServer/IdentityServer4/issues/1331
                var forwardOptions = new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
                    RequireHeaderSymmetry = false
                };

                forwardOptions.KnownNetworks.Clear();
                forwardOptions.KnownProxies.Clear();

                // ref: https://github.com/aspnet/Docs/issues/2384
                app.UseForwardedHeaders(forwardOptions);
            }

            

            app.UseStaticFiles();

            app.UseRouting();
            app.UseIdentityServer();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
