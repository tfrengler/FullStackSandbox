using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace FullStackSandbox.Configuration
{
    public static class Swagger
    {
        public static void ConfigureSwagger(this IServiceCollection self)
        {
            self.AddSwaggerGen(swaggerOptions =>
            {
                swaggerOptions.SwaggerDoc("v1", new OpenApiInfo { Title = "FullStackSandbox", Version = "v1" });
                swaggerOptions.IncludeXmlComments(string.Format(@"{0}\FullStackSandbox.xml", System.AppDomain.CurrentDomain.BaseDirectory));
            });
        }
    }
}
