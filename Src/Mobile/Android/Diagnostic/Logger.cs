using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Diagnostics;
using System.IO;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Diagnostic;
using TinyIoC;

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
        private static Stack<Stopwatch> _watchs = new Stack<Stopwatch>();

        public void LogError(Exception ex)
        {

            LogError(ex, 0);
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
                Write(indentStr + "Error on " + DateTime.Now.ToString());
            }


            Write(indentStr + "Message : " + ex.Message);
            Write(indentStr + "Stack : " + ex.StackTrace);

            if (ex.InnerException != null)
            {
                LogError(ex.InnerException, indent++);
            }
        }

        public void LogStack()
        {

        }
        public void LogMessage(string message , params object[] args)
        {
            if ((args != null) && (args.Length > 0))
            {
                message = string.Format(message, args);
            }

            Write("Message on " + DateTime.Now.ToString() + " : " + message);


        }

       
        public void StartStopwatch(string message)
        {
            var w = new Stopwatch();
            _watchs.Push(w);
            w.Start();
            LogMessage("Start :" + message);
        }


        public void StopStopwatch(string message)
        {
            var w = _watchs.Pop();
            w.Stop();
            LogMessage(message + " Execution time : " + w.ElapsedMilliseconds.ToString() + " ms");
        }

        public readonly static string BaseDir = System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.ToString(), "TaxiHail"); //System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

        public readonly static string LogFilename = System.IO.Path.Combine(BaseDir, "log.txt");

        private static void Write(string message)
        {
            try
            {                
                if (!Directory.Exists(BaseDir))
                {
                    Directory.CreateDirectory(BaseDir);
                }
            }
            catch
            {
                return;
            }


            string version = TinyIoCContainer.Current.Resolve<IPackageInfo>().Version;

            string user = @" N\A with version " + version;

            var msgToLog = message + " by :" + user + " with version " + version;
            
            Console.WriteLine(msgToLog);
            
            
            
            if (File.Exists(LogFilename) && _flushNextWrite)
            {
                File.Delete(LogFilename);
            }

            _flushNextWrite = false;

            try
            {
               
                if (File.Exists (LogFilename)) {
                    var f = new FileInfo (LogFilename);
                    var lenKb = f.Length / 1024;
                    if (lenKb > 375) {
                        File.Delete (LogFilename);
                    }
                }

                using (var fs = new FileStream(LogFilename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    using (var w = new StreamWriter(fs))
                    {
                        w.BaseStream.Seek(0, SeekOrigin.End);
                        w.WriteLine(msgToLog + "\r\n");
                        w.Flush();
                        w.Close();
                    }
                    fs.Close();
                }
            }
            catch
            {

            }

        }

        private static bool _flushNextWrite = false;

        internal static void FlushNextWrite()
        {
            _flushNextWrite = true;
        }
    }
}