using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace MapWinGIS_TOI
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Ghi log lỗi managed để dễ xác định nguyên nhân khi chương trình thoát.
            Application.SetUnhandledExceptionMode(
                UnhandledExceptionMode.CatchException);

            Application.ThreadException +=
                Application_ThreadException;

            AppDomain.CurrentDomain.UnhandledException +=
                CurrentDomain_UnhandledException;

            CrashLogger.Write("===== START APPLICATION =====");
            CrashLogger.Write(
                "Process bitness: " +
                (Environment.Is64BitProcess ? "64-bit" : "32-bit"));

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
            catch (Exception ex)
            {
                CrashLogger.Write("Main fatal exception: " + ex);

                MessageBox.Show(
                    "Chương trình gặp lỗi nghiêm trọng.\n\n" +
                    ex.Message +
                    "\n\nChi tiết đã được ghi vào startup_error.log.",
                    "Lỗi chương trình",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                CrashLogger.Write("===== END APPLICATION =====");
            }
        }

        private static void Application_ThreadException(
            object sender,
            ThreadExceptionEventArgs e)
        {
            CrashLogger.Write(
                "UI ThreadException: " +
                (e.Exception == null ? "(null)" : e.Exception.ToString()));

            MessageBox.Show(
                "Có lỗi trên giao diện.\n\n" +
                (e.Exception == null
                    ? "Không có thông tin lỗi."
                    : e.Exception.Message) +
                "\n\nChi tiết đã được ghi vào startup_error.log.",
                "Lỗi",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        private static void CurrentDomain_UnhandledException(
            object sender,
            UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;

            CrashLogger.Write(
                "AppDomain UnhandledException | terminating=" +
                e.IsTerminating +
                Environment.NewLine +
                (ex == null
                    ? Convert.ToString(e.ExceptionObject)
                    : ex.ToString()));
        }
    }

    internal static class CrashLogger
    {
        private static readonly object SyncRoot = new object();

        internal static string LogPath
        {
            get
            {
                return Path.Combine(
                    Application.StartupPath,
                    "startup_error.log");
            }
        }

        internal static void Write(string message)
        {
            try
            {
                lock (SyncRoot)
                {
                    File.AppendAllText(
                        LogPath,
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") +
                        " | " +
                        message +
                        Environment.NewLine);
                }
            }
            catch
            {
                // Log không được làm chương trình lỗi thêm.
            }
        }
    }
}
