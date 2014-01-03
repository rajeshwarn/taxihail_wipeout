#region

using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

#endregion

namespace MK.DeploymentService
{
    public class BuildLogger : Logger
    {
        private readonly Action<string> _log;

        public BuildLogger(Action<string> log)
        {
            _log = log;
        }

        /// <summary>
        ///     Initialize is guaranteed to be called by MSBuild at the start of the build
        ///     before any events are raised.
        /// </summary>
        public override void Initialize(IEventSource eventSource)
        {
            // For brevity, we'll only register for certain event types. Loggers can also 
            // register to handle TargetStarted/Finished and other events.
            //eventSource.ProjectStarted += new ProjectStartedEventHandler(eventSource_ProjectStarted);
            if (eventSource != null) eventSource.ErrorRaised += eventSource_ErrorRaised;
        }

        private void eventSource_ErrorRaised(object sender, BuildErrorEventArgs e)
        {
            // BuildErrorEventArgs adds LineNumber, ColumnNumber, File, amongst other parameters 
            var line = String.Format(": ERROR {0}({1},{2}, {3}): ", e.File, e.LineNumber, e.ColumnNumber, e.ProjectFile);


            WriteLineWithSenderAndMessage(line, e);
        }

        /// <summary>
        ///     Write a line to the log, adding the SenderName and Message
        ///     (these parameters are on all MSBuild event argument objects)
        /// </summary>
        private void WriteLineWithSenderAndMessage(string line, BuildEventArgs e)
        {
            if (0 == String.Compare(e.SenderName, "MSBuild", StringComparison.OrdinalIgnoreCase))
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
        ///     Just write a line to the log
        /// </summary>
        private void WriteLine(string line, BuildEventArgs e)
        {
            _log(line + e.Message);
        }

        /// <summary>
        ///     Shutdown() is guaranteed to be called by MSBuild at the end of the build, after all
        ///     events have been raised.
        /// </summary>
        public override void Shutdown()
        {
            // Done logging, let go of the file            
        }
    }
}