using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using VaporService.Configuration;
using VaporService.Helpers;
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
            var settingsProvider = new SettingsProvider();
            services.AddSingleton<ISettingsProvider>(settingsProvider);
            services.AddSingleton<ILog>(new ConsoleLog());

            AddUserStorage(services, settingsProvider);
            AddWeaponStorage(services, settingsProvider);

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "VaporService", Version = "v1"});
            });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
            services.AddAuthorization(options =>
            {
                //options.AddPolicy("EmployeeOnly", policy => policy.RequireClaim("EmployeeNumber"));
            });
        }

        private static void AddUserStorage(IServiceCollection services, SettingsProvider settingsProvider)
        {
            services.AddSingleton<IStorage<string, UserData>, GenericFileStorage<string, UserData>>();
            services.AddSingleton<IEntityMapper<string, UserData>>(new EntityMapper<string, UserData>(s => s,
                data => data.ToBytes(),
                bytes => bytes.FromBytes<UserData>(),
                () => settingsProvider.StorageSettings.UserStorageFolder));
        }

        private static void AddWeaponStorage(IServiceCollection services, SettingsProvider settingsProvider)
        {
            services.AddSingleton<IClaimedWeaponIndex, ClaimedWeaponIndex>();
            services.AddSingleton<IStorage<string, Weapon>, GenericFileStorage<string, Weapon>>();
            services.AddSingleton<IEntityMapper<string, Weapon>>(new EntityMapper<string, Weapon>(s => s,
                data => data.ToBytes(),
                bytes => bytes.FromBytes<Weapon>(),
                () => settingsProvider.StorageSettings.WeaponStorageFolder));
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