using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace JawMovementTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            this.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);
        }

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Exception ex = e.Exception.InnerException;
            string text = "Komunikat diagnostyczny z aplikacji JawMovementTool" + Environment.NewLine;
            while (ex != null)
            {
                text += ex.Message + Environment.NewLine;
                ex = ex.InnerException;
            }

            MessageBox.Show(text);
        }

    }
}
