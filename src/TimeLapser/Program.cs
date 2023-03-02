﻿namespace kasthack.TimeLapser
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;

    using Autofac.Extensions.DependencyInjection;

    using kasthack.TimeLapser.Core.Impl.Encoding;
    using kasthack.TimeLapser.Core.Impl.Metadata;
    using kasthack.TimeLapser.Core.Impl.Recorders;
    using kasthack.TimeLapser.Core.Impl.Snappers.Factory;
    using kasthack.TimeLapser.Core.Impl.Snappers.SDGSnapper;
    using kasthack.TimeLapser.Core.Interfaces;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    using Serilog;

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
                .UseSerilog((hostingContext, _, loggerConfiguration) =>
                    loggerConfiguration
                        .ReadFrom.Configuration(hostingContext.Configuration)
                        .Enrich.FromLogContext())

                .Build()
                .RunAsync().ConfigureAwait(false);
        }

        private static IServiceCollection ConfigureServices(IServiceCollection services) => services
                            .AddTransient<IScreenInfoProvider, ScreenInfoProvider>()
                            .AddTransient<IRecorder, ChannelRecorder>()
                            .AddTransient<FrmMain>()
                            .AddTransient<DXSnapper>()
                            .AddTransient<SDGSnapper>()
                            .AddTransient<ISnapperFactory, SnapperFactory>()
                            .AddTransient<IOutputVideoStreamProvider, FfmpegOutputStreamProvider>()
                            .AddHostedService<ApplicationService>();
    }
}
