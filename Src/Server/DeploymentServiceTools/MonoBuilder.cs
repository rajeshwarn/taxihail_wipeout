#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

#endregion

namespace DeploymentServiceTools
{
    public class MonoBuilder
    {
        private readonly Action<string> _logger;

        public MonoBuilder(Action<string> logger)
        {
            _logger = logger;
        }

        public bool ProjectIsInSolution(string slnPath, string projectName)
        {
            var slnContent = File.ReadAllText(slnPath);
            return slnContent.Contains(projectName);
        }

        public void BuildProject(string buildArgs)
        {
            _logger("Running Build - " + buildArgs);

            var buildiOSproject = ProcessEx.GetProcess("/Applications/Xamarin Studio.app/Contents/MacOS/mdtool",
                buildArgs);
            using (var exeProcess = Process.Start(buildiOSproject))
            {
                var output = ProcessEx.GetOutput(exeProcess, 40000);
                if (exeProcess.ExitCode > 0)
                {
                    throw new Exception("Error during build project step" + output.Replace("\n", "\r\n"));
                }
                _logger("Build Successful");
            }
        }

        public void SignAndGenerateBlackBerryProject(string bbToolsPath)
        {
            _logger(" Running Signing BlackBerry ");
            var signArgs = "-Djava.awt.headless=true -Xmx512M -cp \"lib/AndroidTools.jar\" net.rim.tools.apk2bar.Apk2Bar \"Outputs/\" -r";

            var signBBProject = ProcessEx.GetProcess("java",
                signArgs, bbToolsPath);

            using (var exeProcess = Process.Start(signBBProject))
            {
                var output = ProcessEx.GetOutput(exeProcess, 40000);
                if (exeProcess.ExitCode > 0)
                {
                    throw new Exception("Error during build project step" + output.Replace("\n", "\r\n"));
                }
                _logger("Signing Successful");
            }

            _logger(" Running Packaging BlackBerry ");
            var pkgArgs = "-Djava.awt.headless=true -Xmx512M -cp \"lib/BarSigner.jar:lib/AndroidTools.jar\" net.rim.tools.signing.SigningMain -bbidtoken \"Outputs/bbidtoken.csk\" -storepass apcurium72 -keystore \"Outputs/author.p12\" \"Outputs/com.apcurium.MK.TaxiHailDemo-Signed.bar\"";

            var pkgdBBProject = ProcessEx.GetProcess("java",
                pkgArgs, bbToolsPath);

            using (var exeProcess = Process.Start(pkgdBBProject))
            {
                var output = ProcessEx.GetOutput(exeProcess, 40000);
                if (exeProcess.ExitCode > 0)
                {
                    throw new Exception("Error during build project step" + output.Replace("\n", "\r\n"));
                }
                _logger("Packaging Successful");
            }
        }

        public void BuildAndroidProject(List<string> projectLists, string configAndroid, string sln)
        {
            _logger("Build android " + sln);
            var i = 1;
            var count = projectLists.Count;

            foreach (var projectName in projectLists)
            {
                _logger("Step " + (i++) + "/" + count);
                if (!ProjectIsInSolution(sln, projectName + ".csproj"))
                {
                    _logger("Skipping CSPROJ (" + projectName + ") - Not in solution");
                    continue;
                }

                var config = string.Format("\"--project:{0}\" \"--configuration:{1}\"", projectName, configAndroid) +
                             " ";
                var buildArgs = string.Format("build " + config + "\"" + sln + "\"");

                BuildProject(buildArgs);
            }
        }
    }
}