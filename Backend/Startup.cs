using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using FullStackSandbox.Models;
using FullStackSandbox.Configuration;

namespace FullStackSandbox
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
            var SystemConfigSection = Configuration.GetSection("System");
            services.Configure<SystemConfig>(SystemConfigSection);
            var SystemConfig = new Models.SystemConfig();
            SystemConfigSection.Bind(SystemConfig);

            var SecurityConfigSection = Configuration.GetSection("Security");
            services.Configure<SecurityConfig>(SecurityConfigSection);
            var SecurityConfig = new Models.SecurityConfig();
            SecurityConfigSection.Bind(SecurityConfig);

            services.ConfigureCors(SecurityConfig.CORSOrigins, SecurityConfig.CORSAllowedMethods);
            services.ConfigureAuthentication(SecurityConfig);
            services.ConfigureLogging();
            services.ConfigureSwagger();
            services.ConfigureServices(SystemConfig);

            services
                .AddControllers()
                .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            IOptions<SystemConfig>? IntermediateSystemConfig = app.ApplicationServices.GetService<IOptions<SystemConfig>>();
            IOptions<SecurityConfig>? IntermediateSecurityConfig = app.ApplicationServices.GetService<IOptions<SecurityConfig>>();
            Debug.Assert(IntermediateSystemConfig is not null, "System config is null but was expected to yield instance!");
            Debug.Assert(IntermediateSecurityConfig is not null, "Security config is null but was expected to yield instance!");

            SystemConfig SystemConfig = IntermediateSystemConfig.Value;
            SecurityConfig SecurityConfig = IntermediateSecurityConfig.Value;

            logger.LogInformation("ENV NAME: {name}", env.EnvironmentName);
            logger.LogInformation("SYSTEM CONFIG: {newLine}{config}", Environment.NewLine, SystemConfig);
            logger.LogInformation("SECURITY CONFIG: {newLine}{config}", Environment.NewLine, SecurityConfig);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Placeholder v1"));
            }

            app.UseRouting();
            app.UseAuthentication(); // This need to be added before UseAuthorization()
            app.UseAuthorization();
            app.UseCors(Cors.CorsPolicyName);
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}