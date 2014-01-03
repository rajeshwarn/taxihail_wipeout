using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Disposables;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Infrastructure;
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
        private static Stopwatch _stopWatch;

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

        public static void StartStopwatch (string message)
        {
            _stopWatch = new Stopwatch ();
            _stopWatch.Start ();
            
            Write ("Start timer : " + message);
        }

        public static void StopStopwatch (string message)
        {
            if (_stopWatch != null) {
                _stopWatch.Stop ();
                Write ("Stop timer : " + message + " in " + _stopWatch.ElapsedMilliseconds + " ms");
            }
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
            try {
                var user = @" N\A with version " + TinyIoCContainer.Current.Resolve<IPackageInfo> ().Version;
                var account = TinyIoCContainer.Current.Resolve<IAccountService> ().CurrentAccount;
                if (account != null) {
                    user = account.Email;                             
                }
                
                Console.WriteLine (message + " by :" + user + " with version " + TinyIoCContainer.Current.Resolve<IPackageInfo> ().Version);            
            
                if (TinyIoCContainer.Current.Resolve<IAppSettings> ().ErrorLogEnabled) {
                    try {
                        if (File.Exists (TinyIoCContainer.Current.Resolve<IAppSettings> ().ErrorLog)) {
                            var f = new FileInfo (TinyIoCContainer.Current.Resolve<IAppSettings> ().ErrorLog);
                            var lenKb = f.Length / 1024;
                            if (lenKb > 375) {
                                File.Delete (TinyIoCContainer.Current.Resolve<IAppSettings> ().ErrorLog);
                            }
                        }

                        using (var fs = new FileStream (TinyIoCContainer.Current.Resolve<IAppSettings>().ErrorLog, FileMode.OpenOrCreate, FileAccess.ReadWrite)) {
                            using (var w = new StreamWriter (fs)) {
                                w.BaseStream.Seek (0, SeekOrigin.End);
                                w.WriteLine (message + " by :" + user + " with version " + TinyIoCContainer.Current.Resolve<IPackageInfo> ().Version);
                                w.Flush ();
                                w.Close ();
                            }
                            fs.Close ();
                        }
// ReSharper disable once EmptyGeneralCatchClause
                    } catch {
                    
                    }
                }
// ReSharper disable once EmptyGeneralCatchClause
            } catch {
            }
            
        }
    }
}

