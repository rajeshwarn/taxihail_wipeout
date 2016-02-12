using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reactive.Disposables;
using apcurium.MK.Common.Extensions;
using System.Linq;

namespace apcurium.MK.Common.Diagnostic
{
	public abstract class BaseLogger : ILogger
    {
		private static int LogFileMaximumSize = 200 * 1024;

		private static string LogFileName = "taxihail_log.txt";

		private readonly object _threadLock = new object();
		
		public void LogError(Exception ex, string method, int lineNumber)
		{
		    var aggregateException = ex as AggregateException;
            if (aggregateException != null) 
            {
                aggregateException.Handle (x => 
                {
                    LogError (x, 0, method, lineNumber);
                    return true;
                });
            } 
            else 
            {            
                LogError (ex, 0, method, lineNumber);
            }
        }

        public void LogError(Exception ex)
        {
            LogError(ex, string.Empty, -1);
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

        private void LogError(Exception ex, int indent, string method, int lineNumber)
        {
            var indentStr = "";
            for (var i = 0; i < indent; i++)
            {
                indentStr += "   ";
            }
            if (indent == 0)
            {
                var errorMessage = method.HasValueTrimmed() && lineNumber > -1
                    ? string.Format("{0}Error on {1} at {2}:{3}", indentStr, DateTime.Now, method, lineNumber)
                    : string.Format("{0}Error on {1}", indentStr, DateTime.Now);

                Write(errorMessage);
            }

            Write(string.Format ("{0}Message : {1}", indentStr, ex.Message));
            Write(string.Format ("{0}Stack : {1}", indentStr, ex.StackTrace));

            if (ex.InnerException != null)
            {
                LogError(ex.InnerException, ++indent, method, lineNumber);
            }
        }

		protected abstract string GetBaseDirectory();

		protected abstract string GetMessageBase();

		static string RemoveOlderEntries(long overflow, string fileContent)
		{
			var content = fileContent.Split(Environment.NewLine.ToCharArray())
				.ToArray();
			
			var skip = true;

			// We remove the oldest log entries until we have enought space.
			content = content.SkipWhile((line, index) => 
				{
					if (index == 0)
					{
						return true;
					}
					if (!skip)
					{
						return false;
					}
					var previousLinesTotalLenght = content.Take(index).Sum(p => p.Length);

					return overflow - previousLinesTotalLenght > 0;
				})
				.ToArray();
			
			skip = true;

			// We remove enought lines to make sure we don't have a first log element that is truncated.
			var log = content.SkipWhile(line => 
				{
					if (!skip)
					{
						return false;
					}
					skip = !(line.StartsWith("Message on") || line.StartsWith("Error on"));
					return skip;
				})
				.JoinBy(Environment.NewLine).
				Trim();
			
			return log;
		}

		private void DeleteOldEntries (FileInfo fileIO, long overflow)
		{
			var backupFileName = fileIO.FullName + ".bak";

			if(File.Exists(backupFileName))
			{
				File.Delete(backupFileName);
			}

			// Creating a backup copy of the log in case a crash happends.
			File.Copy(fileIO.FullName, backupFileName);

			var fileContent = string.Empty;

			using (var sr = fileIO.OpenText())
			{
				fileContent = sr.ReadToEnd();
			}

			var log = RemoveOlderEntries(overflow, fileContent);


			fileIO.Delete();

			using (var sw = fileIO.CreateText())
			{
				sw.Write(log);
				sw.Write(Environment.NewLine);
				sw.Flush();
			}

			// Deleting the backup copy.
			File.Delete(backupFileName);
		}

		private void Write(string message)
		{
            var messageWithUserName = message + GetMessageBase();

			lock (_threadLock)
			{
				var fileIO = new FileInfo(GetLogFileName());

				using (var sw = fileIO.AppendText())
				{
					sw.WriteLine(messageWithUserName);
					sw.Flush();
				}

				if (fileIO.Length > LogFileMaximumSize)
				{
					DeleteOldEntries(fileIO, fileIO.Length - LogFileMaximumSize);
				}
			}

			Console.WriteLine(messageWithUserName);
		}

		public string GetLogFileName()
		{
			lock (_threadLock)
			{
				if (!Directory.Exists(GetBaseDirectory()))
				{
					Directory.CreateDirectory(GetBaseDirectory());
				}

				var logFileName = Path.Combine(GetBaseDirectory(),LogFileName);

				if (!File.Exists(logFileName) && File.Exists(logFileName + ".bak"))
				{
					File.Copy(logFileName + ".bak", logFileName);
				}

				return logFileName;
			}
		}

		public string MergeLogFiles()
		{
			throw new NotSupportedException();
		}

		public void RemoveMergedFile()
		{
			throw new NotSupportedException();
		}
	}
}