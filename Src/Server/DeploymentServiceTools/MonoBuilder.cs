#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

#endregion

namespace DeploymentServiceTools
{
    public class MonoBuilder
    {
        private readonly Action<string> _logger;
        private const int BuildTimeout = 60000; // one minute

        public MonoBuilder(Action<string> logger)
        {
            _logger = logger;
        }

        public bool ProjectIsInSolution(string slnPath, string projectName)
        {
            var slnContent = File.ReadAllText(slnPath);
            return slnContent.Contains(projectName);
        }

        public void BuildProjectUsingMdTool(string buildArgs)
        {
            _logger("Running mdtool Build - " + buildArgs);

            var buildiOSproject = ProcessEx.GetProcess("/Applications/Xamarin Studio.app/Contents/MacOS/mdtool", buildArgs);
            using (var exeProcess = Process.Start(buildiOSproject))
            {
                var output = ProcessEx.GetOutput(exeProcess, BuildTimeout);
                if (exeProcess.ExitCode > 0)
                {
                    throw new Exception("Error during build project step" + output.Replace("\n", "\r\n"));
                }
                _logger("Build Successful");
            }

            SleepFor(10);
        }

        public void BuildProjectUsingXBuild(string buildArgs)
        {
            _logger("Running xbuild Build - " + buildArgs);

            var buildiOSproject = ProcessEx.GetProcess("/Library/Frameworks/Mono.framework/Commands/xbuild", buildArgs);
            using (var exeProcess = Process.Start(buildiOSproject))
            {
                var output = ProcessEx.GetOutput(exeProcess, BuildTimeout);
                if (exeProcess.ExitCode > 0)
                {
                    throw new Exception("Error during build project step" + output.Replace("\n", "\r\n"));
                }
                _logger("Build Successful");
            }
            SleepFor(10);
        }

        public void SignAndGenerateBlackBerryProject(string bbToolsPath, string barFile)
        {
            _logger(" Running Packaging BlackBerry ");

            var signArgs = "-Djava.awt.headless=true -Xmx512M -cp \"lib/AndroidTools.jar\" net.rim.tools.apk2bar.Apk2Bar \"Outputs/\" -r";
            var signBBProject = ProcessEx.GetProcess("java", signArgs, bbToolsPath);

            using (var exeProcess = Process.Start(signBBProject))
            {
                var output = ProcessEx.GetOutput(exeProcess, BuildTimeout);
                if (exeProcess.ExitCode > 0)
                {
                    throw new Exception("Error during build project step" + output.Replace("\n", "\r\n"));
                }
                _logger("Packaging Successful");
            }

            SleepFor(5);
            _logger(" Running Signing BlackBerry ");

            var pkgArgs = "-Djava.awt.headless=true -Xmx512M -cp \"lib/BarSigner.jar:lib/AndroidTools.jar\" net.rim.tools.signing.SigningMain -bbidtoken \"Outputs/bbidtoken.csk\" -storepass apcurium72 -keystore \"Outputs/author.p12\" \"Outputs/"+barFile+"\"";
            var pkgdBBProject = ProcessEx.GetProcess("java", pkgArgs, bbToolsPath);

            using (var exeProcess = Process.Start(pkgdBBProject))
            {
                var output = ProcessEx.GetOutput(exeProcess, BuildTimeout);
                if (exeProcess.ExitCode > 0)
                {
                    throw new Exception("Error during build project step" + output.Replace("\n", "\r\n"));
                }
                _logger("Signing Successful");
            }

            SleepFor(10);
        }

        private void SleepFor(int seconds)
        {
            Thread.Sleep(seconds * 1000);
        }

    }
}