using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using VaporService.Configuration;
using VaporService.Controllers;
using VaporService.Models;
using VaporService.Storages;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;

namespace VaporService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var log = new ConsoleLog();
            var settingsProvider = new SettingsProvider();
            services.AddSingleton<ISettingsProvider>(settingsProvider);
            services.AddSingleton<ILog>(log);
            services.AddSingleton<IFightForecaster, FightForecaster>();

            AddStorages(services, settingsProvider);

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "VaporService", Version = "v1"});
            });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
            services.AddAuthorization();
        }


        private static void AddStorages(IServiceCollection services, SettingsProvider settingsProvider)
        {
            services.AddSingleton<IClaimesIndex, ClaimsIndex>();
            
            services.AddSingleton<IStorage<string, Fighter>, GenericFileStorage<string, Fighter>>();
            services.AddSingleton<IEntityMapper<string, Fighter>>(new JsonMapper<Fighter>(() => settingsProvider.StorageSettings.UserStorageFolder));
            
            services.AddSingleton<IStorage<string, Weapon>, GenericFileStorage<string, Weapon>>();
            services.AddSingleton<IEntityMapper<string, Weapon>>(new JsonMapper<Weapon>(() => settingsProvider.StorageSettings.WeaponStorageFolder));
            
            services.AddSingleton<IStorage<string, Jabberwocky>, GenericFileStorage<string, Jabberwocky>>();
            services.AddSingleton<IEntityMapper<string, Jabberwocky>>(new JsonMapper<Jabberwocky>(() => settingsProvider.StorageSettings.JabberwockyStorageFolder));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "VaporService v1"));
            }


            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}