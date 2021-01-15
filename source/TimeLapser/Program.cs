using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

namespace kasthack.TimeLapser
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
#if DEBUG
            if (Debugger.IsAttached)
            {
                System.Globalization.CultureInfo.DefaultThreadCurrentCulture =
                    System.Globalization.CultureInfo.DefaultThreadCurrentUICulture =
                        Thread.CurrentThread.CurrentCulture =
                            Thread.CurrentThread.CurrentUICulture
                                = CultureInfo.GetCultureInfo("ru");
            }
#endif
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }
    }
}
