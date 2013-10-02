
using System;
using System.IO;

using System.Diagnostics;
using apcurium.MK.Common.Diagnostic;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.AppServices;
using System.Reactive.Disposables;

namespace apcurium.MK.Booking.Mobile.Client
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
                LogMessage("Stop:  " + message + " Execution time : " + w.ElapsedMilliseconds.ToString() + " ms");
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
    
    public class Logger
    {
        private static Stopwatch _stopWatch;

        public Logger ()
        {
        }

        public static void LogError (Exception ex)
        {
            LogError (ex, 0);
        }

        public  static string GetStack(int position)
        {
            StackTrace stackTrace = new StackTrace();           // get call stack
            StackFrame[] stackFrames = stackTrace.GetFrames();  // get method calls (frames)
            
            return stackFrames[position].GetMethod().Name;        
        }

        public static void LogError (Exception ex, int indent)
        {
            string indentStr = "";
            for (int i = 0; i < indent; i++) {
                indentStr += "   ";
            }
            if (indent == 0) {
                Write (indentStr + "Error on " + DateTime.Now.ToString ());
            }
            
            
            Write (indentStr + "Message : " + ex.Message);
            Write (indentStr + "Stack : " + ex.StackTrace);
            
            if (ex.InnerException != null) {
                LogError (ex.InnerException, indent++);
            }
        }

        public static void LogMessage (string message)
        {
            
            
            Write ("Message on " + DateTime.Now.ToString () + " : " + message);
            
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
            StackTrace stackTrace = new StackTrace ();           // get call stack
            StackFrame[] stackFrames = stackTrace.GetFrames ();  // get method calls (frames)

            // write call stack method names
            foreach (StackFrame stackFrame in stackFrames) {
                if (stackFrame.GetMethod ().Name != "LogStack") {
                    Write ("Stack : " + stackFrame.GetMethod ().Name);   // write method name
                }
            }
        
        }

        private static void Write (string message)
        {
            try {
                string user = @" N\A with version " + TinyIoCContainer.Current.Resolve<IPackageInfo> ().Version;
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
                    } catch {
                    
                    }
                }
            } catch {
            }
            
        }
    }
}

