using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MK.DeploymentService
{
    
    public class BuildLogger     : Logger
    {
        private Action<string> _log;

        public BuildLogger(Action<string> log)
        {
            _log = log;
        }

        /// <summary> 
        /// Initialize is guaranteed to be called by MSBuild at the start of the build 
        /// before any events are raised. 
        /// </summary> 
        public override void Initialize(IEventSource eventSource)
        {
            
            // For brevity, we'll only register for certain event types. Loggers can also 
            // register to handle TargetStarted/Finished and other events.
            //eventSource.ProjectStarted += new ProjectStartedEventHandler(eventSource_ProjectStarted);
            eventSource.ErrorRaised += new BuildErrorEventHandler(eventSource_ErrorRaised);
            
        }

        void eventSource_ErrorRaised(object sender, BuildErrorEventArgs e)
        {
            // BuildErrorEventArgs adds LineNumber, ColumnNumber, File, amongst other parameters 
            string line = String.Format(": ERROR {0}({1},{2}, {3}): ", e.File, e.LineNumber, e.ColumnNumber, e.ProjectFile);

            
            WriteLineWithSenderAndMessage(line, e);
        }

        void eventSource_WarningRaised(object sender, BuildWarningEventArgs e)
        {
            // BuildWarningEventArgs adds LineNumber, ColumnNumber, File, amongst other parameters 
            string line = String.Format(": Warning {0}({1},{2}): ", e.File, e.LineNumber, e.ColumnNumber);
            WriteLineWithSenderAndMessage(line, e);
        }

        void eventSource_MessageRaised(object sender, BuildMessageEventArgs e)
        {
            // BuildMessageEventArgs adds Importance to BuildEventArgs 
            // Let's take account of the verbosity setting we've been passed in deciding whether to log the message 
            if ((e.Importance == MessageImportance.High && IsVerbosityAtLeast(LoggerVerbosity.Minimal))
                || (e.Importance == MessageImportance.Normal && IsVerbosityAtLeast(LoggerVerbosity.Normal))
                || (e.Importance == MessageImportance.Low && IsVerbosityAtLeast(LoggerVerbosity.Detailed))
                )
            {
                WriteLineWithSenderAndMessage(String.Empty, e);
            }
        }

        void eventSource_TaskStarted(object sender, TaskStartedEventArgs e)
        {
            // TaskStartedEventArgs adds ProjectFile, TaskFile, TaskName 
            // To keep this log clean, this logger will ignore these events.
        }

        void eventSource_ProjectStarted(object sender, ProjectStartedEventArgs e)
        {
            // ProjectStartedEventArgs adds ProjectFile, TargetNames 
            // Just the regular message string is good enough here, so just display that.

            WriteLine(e.ProjectFile, e);            
        }

        void eventSource_ProjectFinished(object sender, ProjectFinishedEventArgs e)
        {
            // The regular message string is good enough here too.        
        }

        /// <summary> 
        /// Write a line to the log, adding the SenderName and Message 
        /// (these parameters are on all MSBuild event argument objects) 
        /// </summary> 
        private void WriteLineWithSenderAndMessage(string line, BuildEventArgs e)
        {
            if (0 == String.Compare(e.SenderName, "MSBuild", true /*ignore case*/))
            {
                // Well, if the sender name is MSBuild, let's leave it out for prettiness
                WriteLine(line, e);
            }
            else
            {
                WriteLine(e.SenderName + ": " + line, e);
            }
        }

        /// <summary> 
        /// Just write a line to the log 
        /// </summary> 
        private void WriteLine(string line, BuildEventArgs e)
        {            
            _log(line + e.Message);
        }

        /// <summary> 
        /// Shutdown() is guaranteed to be called by MSBuild at the end of the build, after all  
        /// events have been raised. 
        /// </summary> 
        public override void Shutdown()
        {
            // Done logging, let go of the file            
        }

             
    }
}
