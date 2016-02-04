using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using System.Security.Claims;
using Microsoft.Data.Entity;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using j64.AlarmServer.Models;
using j64.AlarmServer.Services;
using j64.AlarmServer.WebApi.Models;
using Moon.AspNet.Authentication.Basic;

namespace j64.AlarmServer
{
    public class Startup
    {
        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
        {
            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
            }

            AlarmSystemRepository.RepositoryFile = env.MapPath("AlarmSystemInfo.json");
            OauthRepository.RepositoryFile = env.MapPath("SmartThings.json");
            MyLogger.LogFileName = env.MapPath("LogMessages.txt");
            
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            Configuration["Data:DefaultConnection:ConnectionString"] = $@"Data Source={appEnv.ApplicationBasePath}/j64.AlarmServer.db";

        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddEntityFramework()
                .AddSqlite()
                .AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlite(Configuration["Data:DefaultConnection:ConnectionString"]));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders() ;
                
            services.AddAuthorization(options =>
            {
                options.AddPolicy("ArmDisarm", policy => policy.RequireClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "ArmDisarm"));
            });
    
            services.AddMvc();

            // Setup the alarm system
            AlarmSystem alarmSystem = AlarmSystemRepository.Get();
            alarmSystem.ZoneChange += SmartThingsRepository.AlarmSystem_ZoneChange;
            alarmSystem.PartitionChange += SmartThingsRepository.AlarmSystem_PartitionChange;
            alarmSystem.StartSession();

            // Add the alarm system as a service available to the controllers
            services.AddInstance<AlarmSystem>(alarmSystem);
            services.AddTransient<SampleDataInitializer>();
            
            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, SampleDataInitializer sampleData, UserManager<ApplicationUser> userManager)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");

                // For more details on creating database during deployment see http://go.microsoft.com/fwlink/?LinkID=615859
                try
                {
                    using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
                        .CreateScope())
                    {
                        serviceScope.ServiceProvider.GetService<ApplicationDbContext>()
                             .Database.Migrate();
                    }
                }
                catch { }
            }

            app.UseIISPlatformHandler(options => options.AuthenticationDescriptions.Clear());

            app.UseStaticFiles();

            app.UseIdentity();

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
            
            // To configure external authentication please see http://go.microsoft.com/fwlink/?LinkID=532715

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            
            // Seed some default entries into the database
            var task = sampleData.CreateMasterUser();
        }
        // Entry point for the application.
        public static void Main(string[] args) => Microsoft.AspNet.Hosting.WebApplication.Run<Startup>(args);
    }
    
    public class SampleDataInitializer
    {
        private ApplicationDbContext _ctx;
        private UserManager<ApplicationUser> _userManager;
    
        public SampleDataInitializer(ApplicationDbContext ctx, UserManager<ApplicationUser> userManager)
        {
            _ctx = ctx;
            _userManager = userManager;
        }

        public async Task CreateMasterUser( )
        {
            var user = await _userManager.FindByEmailAsync("admin@foo.com");

            if( user == null )
            {
                user = new ApplicationUser()
                {
                    Email = "changeme@changeme.com",
                    UserName = "admin"
                };

                IdentityResult result = await _userManager.CreateAsync(user, "Admin_01");

                if( !result.Succeeded )
                {
                    MyLogger.LogError("Could not create admin user.  Messages were: " + String.Join("; ", result.Errors.Select(x => x.Description)));
                    return;
                }
            }

            var claims = await _userManager.GetClaimsAsync(user);
            await AddClaim(user, claims, "ManageConfig");
            await AddClaim(user, claims, "ArmDisarm");

            MyLogger.LogInfo("Added the default roles to admin user with default password and roles");
        }

        private async Task AddClaim(ApplicationUser user, IList<Claim> claims, string claim )
        {
            var roleType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
            
            if( claims.SingleOrDefault(x => x.Type == roleType && x.Value == claim) == null )
                await _userManager.AddClaimAsync(user, new Claim(roleType, claim, null));
         }
    }
}
