using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Common.Diagnostic
{
	public abstract class BaseLogger : ILogger
    {
	    private const int LogFileMaximumSize = 500*1024;

	    private const string LogFileName = "taxihail_log.txt";

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
            if (stackFrames == null)
            {
                return;
            }

            foreach (var stackFrame in stackFrames.Where(stackFrame => stackFrame.GetMethod().Name != "LogStack"))
            {
                Write(string.Format ("Stack: {0}", stackFrame.GetMethod().Name)); // write method name
            }
        }

        public IDisposable StartStopwatch (string message)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            LogMessage(string.Format("Start: {0}", message));
            return Disposable.Create (() => 
                {
                    stopwatch.Stop();
                    LogMessage(string.Format("Stop: {0} Execution time : {1} ms", message, stopwatch.ElapsedMilliseconds));
                });
        }

		private void LogError(Exception ex, int indent, string method, int lineNumber, IList<string> fullErrorMessage = null)
        {
            var indentStr = "";
            for (var i = 0; i < indent; i++)
            {
                indentStr += "   ";
            }

			if (fullErrorMessage == null)
			{
				fullErrorMessage = new List<string>();
			}

            if (indent == 0)
            {
                var errorMessage = method.HasValueTrimmed() && lineNumber > -1
                    ? string.Format("{0}Error on {1} at {2}:{3}", indentStr, DateTime.Now, method, lineNumber)
                    : string.Format("{0}Error on {1}", indentStr, DateTime.Now);

				fullErrorMessage.Add(errorMessage);
            }

			fullErrorMessage.Add(string.Format ("{0}Message : {1}", indentStr, ex.Message));
			fullErrorMessage.Add(string.Format ("{0}Stack : {1}", indentStr, ex.StackTrace));


			if (ex.InnerException != null)
			{
				LogError(ex.InnerException, ++indent, method, lineNumber, fullErrorMessage);
			}
			else
			{
				Write(fullErrorMessage.JoinBy(Environment.NewLine));
			}
        }

		protected abstract string GetBaseDirectory();

		protected abstract string GetMessageBase();

		private static string RemoveOlderEntries(string fileContent, int overflow)
		{
		    var minTruncateLength = LogFileMaximumSize/8;

            var truncateLength = minTruncateLength < overflow
                ? minTruncateLength + overflow
                : minTruncateLength;

			var content = fileContent.Trim()
                // We truncate at least 1/8 of the max length or 1/8 max legth + overflow
                .Substring(truncateLength)
				.Split(Environment.NewLine.ToCharArray())
				.ToArray();
			
			var skip = true;

			var log = content
				// We remove enought lines to make sure we don't have a log element that is truncated (for instance, a partial stack from an error).
				.SkipWhile(line => 
				{
					if (!skip)
					{
						return false;
					}

					skip = !(line.StartsWith("Message on") || line.StartsWith("Error on"));

					return skip;
				})
				.JoinBy(Environment.NewLine)
                .Trim();
			
			return log;
		}

		private void DeleteOldEntries (FileInfo fileIO, int overflow)
		{
			var backupFileName = fileIO.FullName + ".bak";

		    try
		    {
                if (File.Exists(backupFileName))
                {
                    File.Delete(backupFileName);
				}

				// Creating a backup copy of the log in case a crash happends.
				File.Copy(fileIO.FullName, backupFileName);


		        var fs = fileIO.Open(FileMode.Open, FileAccess.Read);

				var sr = new StreamReader(fs);

				var fileContent = sr.ReadToEnd();

				// Forcing dispose of file handle to attempt to reduce the possibility of "Too many files opened).
				sr.Dispose();
				fs.Close();
				fs.Dispose();

				var log = RemoveOlderEntries(fileContent, overflow);


				fs = fileIO.Open(FileMode.Truncate, FileAccess.Write);
				var sw = new StreamWriter(fs);
				sw.WriteLine(log);
				sw.Flush();

				// Forcing dispose of file handle to attempt to reduce the possibility of "Too many files opened).
				sw.Dispose();
				fs.Close();
				fs.Dispose();

				// Deleting the backup copy.
				File.Delete(backupFileName);
			}
			catch (Exception ex)
			{
				LogError(ex);
			}
		}

		private bool _isWriting;

		private readonly IList<string> _waitingQueue = new List<string>();

		private void Write(string message)
		{
			try
			{
				var messageWithUserName = message + GetMessageBase();

				lock (_threadLock)
				{
					if(_isWriting)
					{
						_waitingQueue.Add(messageWithUserName);
						return;
					}

					_isWriting = true;

					_waitingQueue.Add(messageWithUserName);

					var fileIO = new FileInfo(GetLogFileName());

					if (fileIO.Length + messageWithUserName.Length > LogFileMaximumSize)
					{
						#if DEBUG
						Console.WriteLine("**********************  Log is too long, removing older entries.");
						#endif
						DeleteOldEntries(fileIO, (int)(fileIO.Length + messageWithUserName.Length - LogFileMaximumSize));
					}


					var fs = fileIO.Open(FileMode.Append, FileAccess.Write);

					var sw = new StreamWriter(fs);

					while(_waitingQueue.Any())
					{
						var additionalLine = _waitingQueue.FirstOrDefault();

						_waitingQueue.RemoveAt(0);

						Console.WriteLine(additionalLine);

						sw.WriteLine(additionalLine);
					}

					sw.Flush();

					// Forcing dispose of file handle to attempt to reduce the possibility of "Too many files opened".
					sw.Dispose();
					fs.Close();
					fs.Dispose();

					fileIO = null;
				}
			}
			catch(Exception ex)
			{
				LogError(ex);
			}
			finally
			{
				GC.Collect();
				_isWriting = false;
			}
		}

		public string GetLogFileName()
		{
			lock (_threadLock)
			{
				if (!Directory.Exists(GetBaseDirectory()))
				{
					Directory.CreateDirectory(GetBaseDirectory());
				}

				var logFileName = Path.Combine(GetBaseDirectory(), LogFileName);

				if (!File.Exists(logFileName) && File.Exists(logFileName + ".bak"))
				{
					File.Copy(logFileName + ".bak", logFileName);
				}

				return logFileName;
			}
		}
	}
}