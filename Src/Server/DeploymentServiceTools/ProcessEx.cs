using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeploymentServiceTools
{
    public class ProcessEx
    {
        public static ProcessStartInfo GetProcess(string filename, string args, string workingDirectory = null, bool loadUserProfile = false)
        {
            var p = new ProcessStartInfo
            {
                FileName = filename,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                LoadUserProfile = loadUserProfile,
                Arguments = args
            };
            if (workingDirectory != null)
            {
                p.WorkingDirectory = workingDirectory;
            }
            return p;
        }

        public static string GetOutput(Process exeProcess, int? timeout = null)
        {
            var startTime = DateTime.Now;

            var output = "\n---------------------------------------------\n";

            exeProcess.OutputDataReceived += (s, e) =>
            {
                output += e.Data + "\n";
            };
            exeProcess.ErrorDataReceived += (s, e) =>
            {
                output += e.Data + "\n";
            };

            exeProcess.BeginOutputReadLine();
            exeProcess.BeginErrorReadLine();


            while (!exeProcess.HasExited)
            {
                if (timeout.HasValue)
                {
                    if ((DateTime.Now - startTime).TotalSeconds > timeout.Value)
                    {
                        throw new Exception("Build Timeout, " + output);
                    }
                }
                //todo Hack -- Wait for exit seems to lag for project builds
            }

            return output += "\n-----------------------------------Ran For: " + (DateTime.Now - startTime).TotalSeconds + "s----------Code:" + exeProcess.ExitCode + "\n";
        }

    }
}
