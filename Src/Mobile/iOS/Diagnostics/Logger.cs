using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Disposables;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using TinyIoC;

namespace apcurium.MK.Booking.Mobile.Client.Diagnostics
{
    public class LoggerWrapper :  ILogger
    {
        public void LogError (Exception ex)
        {
            if (ex is AggregateException) {
                ((AggregateException)ex).Handle (x => {
                    Logger.LogError (x);
                    return true;
                });
            } else {            
                Logger.LogError (ex);
            }
        }
        
        public void LogMessage (string message, params object[] args)
        {
            if ((args != null) && (args.Length > 0)) {
                message = string.Format (message, args);
            }

            Logger.LogMessage (message);

        }
        
        public IDisposable StartStopwatch(string message)
        {
            var w = new Stopwatch();
            w.Start();
            LogMessage("Start: " + message);
            return Disposable.Create (() => {
                w.Stop();
                LogMessage("Stop:  " + message + " Execution time : " + w.ElapsedMilliseconds + " ms");
            });
        }

        public void LogStack ()
        {
            Logger.LogStack ();
        }

        public string GetStack(int position)
        {
         	return Logger.GetStack (position+1);
        }

    }
    
    public static class Logger
    {
        public static void LogError (Exception ex)
        {
            LogError (ex, 0);
        }

        public static string GetStack(int position)
        {
            var stackTrace = new StackTrace();           // get call stack
            var stackFrames = stackTrace.GetFrames();  // get method calls (frames)

            if (stackFrames != null)
            {
                return stackFrames[position].GetMethod().Name;
            }
            return "no stack frame";
        }

        public static void LogError (Exception ex, int indent)
        {
            var indentStr = "";
            for (var i = 0; i < indent; i++) {
                indentStr += "   ";
            }
            if (indent == 0) {
                Write (indentStr + "Error on " + DateTime.Now);
            }
            
            
            Write (indentStr + "Message : " + ex.Message);
            Write (indentStr + "Stack : " + ex.StackTrace);
            
            if (ex.InnerException != null) {
// ReSharper disable once RedundantAssignment
                LogError (ex.InnerException, indent++);
            }
        }

        public static void LogMessage (string message)
        {
            Write ("Message on " + DateTime.Now + " : " + message);
        }

        public static void LogStack ()
        {
            var stackTrace = new StackTrace ();           // get call stack
            var stackFrames = stackTrace.GetFrames ();  // get method calls (frames)

            // write call stack method names
            if (stackFrames != null)
                foreach (var stackFrame in stackFrames) {
                    if (stackFrame.GetMethod ().Name != "LogStack") {
                        Write ("Stack : " + stackFrame.GetMethod ().Name);   // write method name
                    }
                }
        }

        private static void Write (string message)
        {
            try
            {
                var settings = TinyIoCContainer.Current.Resolve<IAppSettings> ().Data;
                var documents = Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);
                var filePath = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments), "..", "Library", settings.ErrorLogFile);
                var account = TinyIoCContainer.Current.Resolve<IAccountService> ().CurrentAccount;
                var user = account == null
                    ? @" N\A "
                    : account.Email;

                message += " by :" + user + " with version " + TinyIoCContainer.Current.Resolve<IPackageInfo> ().Version;

                Console.WriteLine (message);            
            
                if (settings.ErrorLogEnabled)
                {
                    if (File.Exists (filePath))
                    {
                        var f = new FileInfo (filePath);
                        var lenKb = f.Length / 1024;
                        if (lenKb > 375)
                        {
                            f.Delete();
                        }
                    }

                    File.AppendAllLines(filePath, new[] { message });
                }

            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
        }
    }
}

