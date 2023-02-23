﻿namespace kasthack.TimeLapser
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    using Microsoft.Extensions.Hosting;

    internal record ApplicationService(Func<FrmMain> FormFactory, IHostApplicationLifetime ApplicationLifetime) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            _ = Task.Run(() => this.DoWork());
        }

        private async Task DoWork()
        {
            await Task.CompletedTask;
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(this.FormFactory());
            this.ApplicationLifetime.StopApplication();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            Application.Exit();
        }
    }
}
