using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reactive.Disposables;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Common.Diagnostic
{
    public abstract class BaseLogger : ILogger
    {
		private static int LogFileMaximumSize = 200 * 1024;

		private static string FirstLogFileName = "taxihail_log_1.txt";
		private static string SecondLogFileName = "taxihail_log_2.txt";
		private static string MergedLogFileName = "taxihail_log.txt";

		private static int RetainMergedFileTime = 1; // hours

		private string _firstLogFileFullName;
		private string _secondLogFileFullName;
		private string _mergedLogFileFullName;
		private string _activeFileFullName;
		private string _inactiveFileFullName;

		private readonly object _fileSwitchExclusiveAccess = new object();
		
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

		private void SetActiveLogFile(long nextMessageLength)
		{
			if (!Directory.Exists(GetBaseDirectory()))
			{
				Directory.CreateDirectory(GetBaseDirectory());
			}

			if (_firstLogFileFullName == null || _secondLogFileFullName == null || _mergedLogFileFullName == null)
			{
				_firstLogFileFullName = Path.Combine(GetBaseDirectory(), FirstLogFileName);
				_secondLogFileFullName = Path.Combine(GetBaseDirectory(), SecondLogFileName);

				_activeFileFullName = _firstLogFileFullName;
				_inactiveFileFullName = _secondLogFileFullName;

				_mergedLogFileFullName = Path.Combine(GetBaseDirectory(), MergedLogFileName);
			}

			long activeFileLength = 0;

			if (File.Exists(_activeFileFullName))
			{
				var activeFileInfo = new FileInfo(_activeFileFullName);
				activeFileLength = activeFileInfo.Length;
			}

			if (activeFileLength + nextMessageLength >= LogFileMaximumSize)
			{
				if (File.Exists(_inactiveFileFullName))
				{
					File.Delete(_inactiveFileFullName);
				}

				var tempFullName = _activeFileFullName;
				_activeFileFullName = _inactiveFileFullName;
				_inactiveFileFullName = tempFullName;
			}

			if (File.Exists(_mergedLogFileFullName) && (DateTime.Now - (new FileInfo(_mergedLogFileFullName)).LastWriteTime).TotalHours >= RetainMergedFileTime)
			{
				File.Delete(_inactiveFileFullName);
			}
		}

		private void Write(string message)
		{
            var messageWithUserName = message + GetMessageBase();


			lock (_fileSwitchExclusiveAccess)
			{
				SetActiveLogFile(messageWithUserName.Length);

				File.AppendAllLines(_activeFileFullName, new[] { messageWithUserName });
			}


			Console.WriteLine(messageWithUserName);
		}

		private string[] GetLogFilesFullName()
		{
			SetActiveLogFile(0);

			var nonEmptyLogs = new List<string>();

			if (File.Exists(_inactiveFileFullName) && (new FileInfo(_inactiveFileFullName)).Length > 0)
			{
				nonEmptyLogs.Add(_inactiveFileFullName);
			}

			if (File.Exists(_activeFileFullName) && (new FileInfo(_activeFileFullName)).Length > 0)
			{
				nonEmptyLogs.Add(_activeFileFullName);
			}

			return nonEmptyLogs.ToArray();
		}

		public string MergeLogFiles()
		{
			lock (_fileSwitchExclusiveAccess)
			{
				var logFiles = GetLogFilesFullName();

			    if (logFiles.Length <= 0)
			    {
			        return null;
			    }

			    var mergedLogFile = Path.Combine(GetBaseDirectory(), MergedLogFileName);

			    File.Copy(logFiles[0], mergedLogFile, true);

			    for (var i = 1; i < logFiles.Length; i++)
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

		public void RemoveMergedFile()
		{
			lock (_fileSwitchExclusiveAccess)
			{
				if (_mergedLogFileFullName != null && File.Exists(_mergedLogFileFullName))
				{
					File.Delete(_mergedLogFileFullName);
				}
			}
		}
	}
}