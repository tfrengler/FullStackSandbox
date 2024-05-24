using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace FullStackSandbox.Configuration
{
    public static class Cors
    {
        public const string CorsPolicyName = "default";

        public static void ConfigureCors(this IServiceCollection self, string[] origins, string[] allowedMethods)
        {
            Debug.Assert(origins is not null);
            Debug.Assert(allowedMethods is not null);

            self.AddCors(options =>
            {
                options.AddPolicy(
                    name: CorsPolicyName,
                    policy =>
                    {
                        policy
                            .AllowCredentials()
                            .WithOrigins(origins)
                            .WithMethods(allowedMethods);
                    }
                );
            });
        }
    }
}
