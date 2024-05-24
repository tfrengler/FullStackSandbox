using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FullStackSandbox.Configuration
{
    public static class Logging
    {
        public static void ConfigureLogging(this IServiceCollection self)
        {
            self.AddLogging(configuration => configuration.AddSimpleConsole(config => config.TimestampFormat = "[dd-MM-yyyy HH:mm:ss] - "));
        }
    }
}
