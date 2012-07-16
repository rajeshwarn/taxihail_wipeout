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
        private static Stopwatch _stopWatch;
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
        public void LogMessage(string message)
        {


            Write("Message on " + DateTime.Now.ToString() + " : " + message);


        }

        public void StartStopwatch(string message)
        {
            _stopWatch = new Stopwatch();
            _stopWatch.Start();

            Write("Start timer : " + message);
        }

        public void StopStopwatch(string message)
        {
            if (_stopWatch != null)
            {
                _stopWatch.Stop();
                Write("Stop timer : " + message + " in " + _stopWatch.ElapsedMilliseconds + " ms");
            }
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
            string user = @" N\A with version " + AppSettings.Version;
            
            var msgToLog = message + " by :" + user + " with version " + AppSettings.Version;
            
            Console.WriteLine(msgToLog);
            
            
            
            if (File.Exists(LogFilename) && _flushNextWrite)
            {
                File.Delete(LogFilename);
            }

            _flushNextWrite = false;

            try
            {
               
                using (var fs = new FileStream(LogFilename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    using (var w = new StreamWriter(fs))
                    {
                        w.BaseStream.Seek(0, SeekOrigin.End);
                        w.WriteLine(message +"\r\n");
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