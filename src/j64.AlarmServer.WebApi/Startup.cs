using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using j64.AlarmServer;
using j64.AlarmServer.WebApi;
using Microsoft.AspNet.Authorization;

namespace j64.AlarmServer.WebApi
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
                builder.AddUserSecrets();

            Model.AlarmSystemRepository.RepositoryFile = env.MapPath("AlarmSystemInfo.json");
            Model.OauthRepository.RepositoryFile = env.MapPath("SmartThings.json");
            MyLogger.LogFileName = env.MapPath("LogMessages.txt");

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            // Setup the alarm system
            AlarmSystem alarmSystem = Model.AlarmSystemRepository.Get();
            alarmSystem.ZoneChange += Model.SmartThingsRepository.AlarmSystem_ZoneChange;
            alarmSystem.PartitionChange += Model.SmartThingsRepository.AlarmSystem_PartitionChange;
            alarmSystem.StartSession();

            // Add the alarm system as a service available to the controllers
            services.AddInstance<AlarmSystem>(alarmSystem);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseIISPlatformHandler();

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        // Entry point for the application.
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
