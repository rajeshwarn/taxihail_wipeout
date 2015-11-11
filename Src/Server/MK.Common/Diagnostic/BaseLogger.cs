using apcurium.MK.Booking.Mobile.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reactive.Disposables;

namespace apcurium.MK.Common.Diagnostic
{
    public abstract class BaseLogger : ILogger
    {
		private static int LogFileMaximumSize = 200 * 1024;

		private static string FirstLogFileName = "taxihail_log_1.txt";
		private static string SecondLogFileName = "taxihail_log_2.txt";
		private static string MergedLogFileName = "taxihail_log.txt";

		private static int RetainMergedFileTime = 1; // hours

		private string firstLogFileFullName;
		private string secondLogFileFullName;
		private string mergedLogFileFullName;
		private string activeFileFullName;
		private string inactiveFileFullName;

		private object _fileSwitchExclusiveAccess = new object();

		private string messageBase;
		
		public void LogError(Exception ex)
        {
            if (ex is AggregateException) 
            {
                ((AggregateException)ex).Handle (x => 
                {
                    LogError (x, 0);
                    return true;
                });
            } 
            else 
            {            
                LogError (ex, 0);
            }
        }

        public void LogMessage(string message, params object[] args)
        {
            if ((args != null) && (args.Length > 0))
            {
                message = string.Format(message, args);
            }

            Write(string.Format ("Message on {0} : {1}", DateTime.Now, message));
        }

        public void LogStack ()
        {
            var stackTrace = new StackTrace();           // get call stack
            var stackFrames = stackTrace.GetFrames();    // get method calls (frames)

            // write call stack method names
            if (stackFrames != null)
            {
                foreach (var stackFrame in stackFrames)
                {
                    if (stackFrame.GetMethod().Name != "LogStack")
                    {
                        Write(string.Format ("Stack: {0}", stackFrame.GetMethod().Name)); // write method name
                    }
                }
            }
        }

        public IDisposable StartStopwatch (string message)
        {
            var w = new Stopwatch();
            w.Start();
            LogMessage(string.Format("Start: {0}", message));
            return Disposable.Create (() => 
                {
                    w.Stop();
                    LogMessage(string.Format("Stop: {0} Execution time : {1} ms", message, w.ElapsedMilliseconds));
                });
        }

        private void LogError(Exception ex, int indent)
        {
            string indentStr = "";
            for (int i = 0; i < indent; i++)
            {
                indentStr += "   ";
            }
            if (indent == 0)
            {
                Write(string.Format("{0}Error on {1}", indentStr, DateTime.Now));
            }

            Write(string.Format ("{0}Message : {1}", indentStr, ex.Message));
            Write(string.Format ("{0}Stack : {1}", indentStr, ex.StackTrace));

            if (ex.InnerException != null)
            {
                LogError(ex.InnerException, ++indent);
            }
        }

		protected abstract string GetBaseDirectory();

		protected abstract string GetMessageBase();

		private void SetActiveLogFile(long nextMessageLength)
		{
			if (!Directory.Exists(GetBaseDirectory()))
			{
				Directory.CreateDirectory(GetBaseDirectory());
			}

			if (firstLogFileFullName == null || secondLogFileFullName == null || mergedLogFileFullName == null)
			{
				firstLogFileFullName = Path.Combine(GetBaseDirectory(), FirstLogFileName);
				secondLogFileFullName = Path.Combine(GetBaseDirectory(), SecondLogFileName);

				activeFileFullName = firstLogFileFullName;
				inactiveFileFullName = secondLogFileFullName;

				mergedLogFileFullName = Path.Combine(GetBaseDirectory(), MergedLogFileName);
			}

			long activeFileLength = 0;

			if (File.Exists(activeFileFullName))
			{
				var activeFileInfo = new FileInfo(activeFileFullName);
				activeFileLength = activeFileInfo.Length;
			}

			if (activeFileLength + nextMessageLength >= LogFileMaximumSize)
			{
				if (File.Exists(inactiveFileFullName))
				{
					File.Delete(inactiveFileFullName);
				}

				var tempFN = activeFileFullName;
				activeFileFullName = inactiveFileFullName;
				inactiveFileFullName = tempFN;
			}

			if (File.Exists(mergedLogFileFullName) && (DateTime.Now - (new FileInfo(mergedLogFileFullName)).LastWriteTime).TotalHours >= RetainMergedFileTime)
			{
				File.Delete(inactiveFileFullName);
			}
		}

		private void Write(string message)
		{
			if (messageBase == null)
			{
				messageBase = GetMessageBase();
			}

			var messageWithUserName = message + messageBase;


			lock (_fileSwitchExclusiveAccess)
			{
				SetActiveLogFile(messageWithUserName.Length);

				File.AppendAllLines(activeFileFullName, new[] { messageWithUserName });
			}


			Console.WriteLine(messageWithUserName);
		}

		private string[] GetLogFilesFullName()
		{
			SetActiveLogFile(0);

			var nonEmptyLogs = new List<string>();

			if (File.Exists(inactiveFileFullName) && (new FileInfo(inactiveFileFullName)).Length > 0)
			{
				nonEmptyLogs.Add(inactiveFileFullName);
			}

			if (File.Exists(activeFileFullName) && (new FileInfo(activeFileFullName)).Length > 0)
			{
				nonEmptyLogs.Add(activeFileFullName);
			}

			return nonEmptyLogs.ToArray();
		}

		public string MergeLogFiles()
		{
			lock (_fileSwitchExclusiveAccess)
			{
				var logFiles = GetLogFilesFullName();

				if (logFiles.Length > 0)
				{
					var mergedLogFile = Path.Combine(GetBaseDirectory(), MergedLogFileName);

					File.Copy(logFiles[0], mergedLogFile, true);

					for (int i = 1; i < logFiles.Length; i++)
					{
						var currentLogStream = File.OpenRead(logFiles[i]);
						var currentStreamReader = new StreamReader(currentLogStream);

						var h = currentStreamReader.ReadToEnd();
						currentStreamReader.Close();
						currentLogStream.Close();

						File.AppendAllText(mergedLogFile, h);
					}

					return mergedLogFile;
				}
			}

			return null;
		}

		public void RemoveMergedFile()
		{
			lock (_fileSwitchExclusiveAccess)
			{
				if (mergedLogFileFullName != null && File.Exists(mergedLogFileFullName))
				{
					File.Delete(mergedLogFileFullName);
				}
			}
		}
	}
}