namespace kasthack.TimeLapser
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Threading;
    using System.Windows.Forms;

    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
#if DEBUG
            if (Debugger.IsAttached)
            {
                CultureInfo.DefaultThreadCurrentCulture =
                    CultureInfo.DefaultThreadCurrentUICulture =
                        Thread.CurrentThread.CurrentCulture =
                            Thread.CurrentThread.CurrentUICulture
                                = CultureInfo.GetCultureInfo("ru");
            }
#endif
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FrmMain());
        }
    }
}
