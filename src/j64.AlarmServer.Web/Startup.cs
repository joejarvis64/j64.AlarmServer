using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using j64.AlarmServer.Web.Data;
using j64.AlarmServer.Web.Models;
using j64.AlarmServer.Web.Services;
using j64.AlarmServer.Web.Repository;
using Microsoft.AspNet.Builder;
using Moon.AspNet.Authentication.Basic;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace j64.AlarmServer.Web
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
            }

            // Setup up the config file paths
            Repository.AlarmSystemRepository.RepositoryFile = $"{env.WebRootPath}/AlarmSystemInfo.json";
            Repository.OauthRepository.RepositoryFile = $"{env.WebRootPath}/SmartThings.json";

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var s = services.FirstOrDefault(x => x.ServiceType == typeof(IHostingEnvironment));
            var env = s.ImplementationInstance as IHostingEnvironment;

            // Add framework services.
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite($@"Data Source={env.ContentRootPath}/j64.AlarmServer.db"));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("ArmDisarm", policy => policy.RequireClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "ArmDisarm"));
            });

            services.AddMvc();

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();

            // Add a single instance of the alarm system
            var alarmSystem = Repository.AlarmSystemRepository.Get();
            alarmSystem.ZoneChange += SmartThingsRepository.AlarmSystem_ZoneChange;
            alarmSystem.PartitionChange += SmartThingsRepository.AlarmSystem_PartitionChange;
            alarmSystem.StartSession();
            services.AddSingleton<AlarmSystem>(alarmSystem);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, UserManager<ApplicationUser> userManager, ApplicationDbContext ctx)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseIdentity();

            // Add external authentication middleware below. To configure them please see http://go.microsoft.com/fwlink/?LinkID=532715
            // Refactor this into a seperate class
            // Remove hard coding of the password in the installDevices routine!
            app.UseBasicAuthentication(o =>
            {
                o.Realm = $"j64 Alarm";

                o.Events = new BasicAuthenticationEvents
                {
                    OnSignIn = c =>
                    {
                        var x = userManager.FindByNameAsync(c.UserName);
                        x.Wait();
                        if (x.Result != null)
                        {
                            var y = userManager.CheckPasswordAsync(x.Result, c.Password);
                            y.Wait();

                            if (y.Result == true)
                            {
                                var z = userManager.GetClaimsAsync(x.Result);
                                z.Wait();
                                var identity = new ClaimsIdentity(z.Result, c.Options.AuthenticationScheme);
                                c.Principal = new ClaimsPrincipal(identity);
                            }
                        }

                        return Task.FromResult(true);
                    }
                };
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });


            // Seed some default entries into the database
            var task = new Data.UserDataInitializer(ctx, userManager).CreateMasterUser();
        }
    }
}
