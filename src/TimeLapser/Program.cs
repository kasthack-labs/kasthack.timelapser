namespace kasthack.TimeLapser
{
    using System;
    using System.Diagnostics.Metrics;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Transactions;

    using Autofac.Extensions.DependencyInjection;
    using kasthack.TimeLapser.Recording.Metadata;
    using kasthack.TimeLapser.Recording.Recorder;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    using Serilog;
    using Serilog.Events;

    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static async Task Main(string[] args)
        {
            var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            await
                Host
                .CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(configuration => configuration.AddJsonFile(Path.Combine(assemblyDirectory, "appSettings.json")))
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureServices(services => ConfigureServices(services))
                .UseSerilog((hostingContext, services, loggerConfiguration) =>
                    loggerConfiguration
                        .ReadFrom.Configuration(hostingContext.Configuration)
                        .Enrich.FromLogContext())

                .Build()
                .RunAsync();
        }

        private static IServiceCollection ConfigureServices(IServiceCollection services) => services
                            .AddTransient<IScreenInfoProvider, ScreenInfoProvider>()
                            .AddTransient<IRecorder, Recorder>()
                            .AddTransient<FrmMain>()
                            .AddHostedService<ApplicationService>();
    }
}
