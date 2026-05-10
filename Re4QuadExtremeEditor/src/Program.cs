using System;
using System.Windows.Forms;

namespace Re4QuadExtremeEditor
{
    static class Program
    {
        /// <summary>
        /// Main entry point. Enables visual styles, installs a top-level
        /// exception handler so unhandled errors show a friendly message
        /// instead of crashing silently, then launches the main window.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Catch unhandled UI-thread exceptions and show a dialog instead
            // of letting the process crash without any feedback to the user.
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (sender, e) =>
            {
                MessageBox.Show(
                    "An unexpected error occurred:\n\n" + e.Exception.Message,
                    "Unexpected Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            };

            // Catch unhandled non-UI-thread exceptions.
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                string msg = e.ExceptionObject is Exception ex
                    ? ex.Message
                    : e.ExceptionObject?.ToString() ?? "Unknown error";
                MessageBox.Show(
                    "A fatal error occurred:\n\n" + msg,
                    "Fatal Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            };

            Application.Run(new MainForm());
        }
    }
}
