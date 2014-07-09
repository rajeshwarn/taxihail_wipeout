using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Disposables;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using TinyIoC;
using Environment = Android.OS.Environment;

namespace apcurium.MK.Booking.Mobile.Client.Diagnostic
{
    public static class Logger
    {
        public static void LogError(Exception ex)
        {
            new LoggerImpl().LogError(ex);
        }

        public static void LogMessage(string message)
        {
            new LoggerImpl().LogMessage(message);
        }
    }

    public class LoggerImpl : ILogger
    {
        public static readonly string BaseDir =
            Path.Combine(Environment.ExternalStorageDirectory.ToString(), "TaxiHail");

        public static string LogFilename;
        private static bool _flushNextWrite;

        public void LogError(Exception ex)
        {
            LogError(ex, 0);
        }


        public void LogStack()
        {
            var stackTrace = new StackTrace(); // get call stack
            StackFrame[] stackFrames = stackTrace.GetFrames(); // get method calls (frames)

            // write call stack method names
            if (stackFrames != null)
                foreach (var stackFrame in stackFrames)
                {
                    if (stackFrame.GetMethod().Name != "LogStack")
                    {
                        Write("Stack : " + stackFrame.GetMethod().Name); // write method name
                    }
                }
        }

        public string GetStack(int position)
        {
            var stackTrace = new StackTrace(); // get call stack
            StackFrame[] stackFrames = stackTrace.GetFrames(); // get method calls (frames)

            return stackFrames != null
                ? stackFrames[position].GetMethod().Name
                : "stack frame null";
        }

        public void LogMessage(string message, params object[] args)
        {
            if ((args != null) && (args.Length > 0))
            {
                message = string.Format(message, args);
            }

            Write("Message on " + DateTime.Now + " : " + message);
        }


        public IDisposable StartStopwatch(string message)
        {
            var w = new Stopwatch();
            w.Start();
            LogMessage("Start: " + message);
            return Disposable.Create(() =>
            {
                w.Stop();
                LogMessage("Stop:  " + message + " Execution time : " + w.ElapsedMilliseconds + " ms");
            });
        }

        public void LogError(Exception ex, int indent)
        {
            string indentStr = "";
            for (int i = 0; i < indent; i++)
            {
                indentStr += "   ";
            }
            if (indent == 0)
            {
                Write(indentStr + "Error on " + DateTime.Now);
            }


            Write(indentStr + "Message : " + ex.Message);
            Write(indentStr + "Stack : " + ex.StackTrace);

            if (ex.InnerException != null)
            {
// ReSharper disable once RedundantAssignment
                LogError(ex.InnerException, indent++);
            }
        }

        private static void Write(string message)
        {
            try
            {
                if (!Directory.Exists(BaseDir))
                {
                    Directory.CreateDirectory(BaseDir);
                }            

                var user = @" N\A ";
                if(TinyIoCContainer.Current.CanResolve<IAccountService>()
                    && TinyIoCContainer.Current.Resolve<IAccountService> ().CurrentAccount != null)
                {
                    user = TinyIoCContainer.Current.Resolve<IAccountService> ().CurrentAccount.Email;
                }                 
                var settings = TinyIoCContainer.Current.Resolve<IAppSettings> ().Data;
                var packageInfo = TinyIoCContainer.Current.Resolve<IPackageInfo>();

                

                message += string.Format(" by : {0} with version {1} - company {2} - platform {3}",
                    user,
                    packageInfo.Version,
                    settings.ApplicationName,
					packageInfo.PlatformDetails);

                Console.WriteLine(message);

            }
            catch
            {
                return;
            }

            LogFilename = "errorlog.txt";
            if(TinyIoCContainer.Current.CanResolve<IAppSettings>())
            {
                LogFilename = Path.Combine(BaseDir, "errorlog.txt");
            }

            if (File.Exists(LogFilename) && _flushNextWrite)
            {
                File.Delete(LogFilename);
            }

            _flushNextWrite = false;

            try
            {
                if (File.Exists(LogFilename))
                {
                    var f = new FileInfo(LogFilename);
                    var lenKb = f.Length/1024;
                    if (lenKb > 375)
                    {
                        File.Delete(LogFilename);
                    }
                }

                using (var fs = new FileStream(LogFilename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    using (var w = new StreamWriter(fs))
                    {
                        w.BaseStream.Seek(0, SeekOrigin.End);
                        w.WriteLine(message + "\r\n");
                        w.Flush();
                        w.Close();
                    }
                    fs.Close();
                }
            }
// ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }
        }

        internal static void FlushNextWrite()
        {
            _flushNextWrite = true;
        }
    }
}