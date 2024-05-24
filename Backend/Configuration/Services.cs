using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace FullStackSandbox.Configuration
{
    public static class Services
    {
        public static void ConfigureServices(this IServiceCollection self, Models.SystemConfig config)
        {
            Debug.Assert(config is not null);

            self.AddSingleton<FullStackSandbox.Services.JwtService>();
            self.AddSingleton(_ => new FullStackSandbox.Services.DatabaseService(new System.IO.FileInfo(config.DatabaseFile)));
            self.AddSingleton<FullStackSandbox.Services.UserService>();
        }
    }
}
