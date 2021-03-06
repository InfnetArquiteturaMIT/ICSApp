using ICSApp.IncidentService.Application.Interfaces;
using ICSApp.IncidentService.Infra.Context;
using ICSApp.IncidentService.Infra.Repositories;
using ICSApp.IncidentService.Infra.UnitsOfWork;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net.Http.Headers;

namespace ICSApp.IncidentService.WebClient
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var connection = Configuration.GetConnectionString("Connection");

            services.AddDbContext<DbContext, IncidentDbContext>(options =>
            {
                options.UseMySQL(connection, b => b.MigrationsAssembly("ICSApp.IncidentService.Infra"));
            });

            services.AddHttpClient("Activity Microservice", client =>
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.BaseAddress = new Uri("https://localhost:44300/");
            });

            services.AddScoped<IUnitOfWork, IncidentUnitOfWork>();
            services.AddScoped<IUnitOfWork, MemberUnitOfWork>();
            services.AddScoped<IUnitOfWork, FunctionUnitOfWork>();
            services.AddScoped<IRepository, IncidentRepository>();
            services.AddScoped<IRepository, MemberRepository>();
            services.AddScoped<IRepository, FunctionRepository>();
            services.AddTransient<IIncidentService, Application.Services.IncidentService>();
            services.AddTransient<IMemberService, Application.Services.MemberService>();
            services.AddTransient<IFunctionService, Application.Services.FunctionService>();

            services.AddControllers();

            services.AddMvcCore(opt => {
                opt.EnableEndpointRouting = false;
            });

            services.AddAuthorization();
            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = "https://localhost:44310";
                    options.RequireHttpsMetadata = false;
                    options.ApiName = "IncidentMicroservice_ResourceApi";

                });
        }
                
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseAuthentication();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseMvc();
        }
    }
}
