using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace FullStackSandbox
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            IHost OurHost = Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseKestrel((hostingContext, options) =>
                {
                    options.ListenLocalhost(5000);
                });
                webBuilder.UseStartup<Startup>();
            })
            .Build();

            Console.WriteLine("Host configured and built");
            OurHost.Run();
        }
    }
}