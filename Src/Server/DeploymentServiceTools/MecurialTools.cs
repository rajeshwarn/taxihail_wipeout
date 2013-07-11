using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace DeploymentServiceTools
{
    public class MecurialTools
    {
        private readonly string _sourceDirectory;
        public string HgPath { get; set; }

        public MecurialTools(string hgPath,string sourceDirectory)
        {
            HgPath = hgPath;
            _sourceDirectory = sourceDirectory;
            
        }

        public void Pull()
        {             
            var hgPull = ProcessEx.GetProcess(HgPath, string.Format("pull https://buildapcurium:apcurium5200!@bitbucket.org/apcurium/mk-taxi --repository {0}", _sourceDirectory));
            using (var exeProcess = Process.Start(hgPull))
            {
                var output = ProcessEx.GetOutput(exeProcess);
                if (exeProcess.ExitCode > 0)
                {
                    throw new Exception("Error during pull source code step" + output);
                }
            }
        }

        private static string GetRevisionString(string revisionNumber)
        {
            var revision = string.IsNullOrEmpty(revisionNumber) ? string.Empty : "-r " + revisionNumber;
            return revision;
        }

        public void Clone(string revisionNumber)
        {
            var revision = GetRevisionString(revisionNumber);
            
            var args = string.Format(@"clone {1} https://buildapcurium:apcurium5200!@bitbucket.org/apcurium/mk-taxi {0}",
                                     _sourceDirectory, revision);

            var hgClone = ProcessEx.GetProcess(HgPath, args);
            using (var exeProcess = Process.Start(hgClone))
            {
                var output = ProcessEx.GetOutput(exeProcess);
                if (exeProcess.ExitCode > 0)
                {
                    throw new Exception("Error during clone source code step" + output);
                }
            }
        }

        public string GetTipRevisionNumber()
        {
            var identify = new ProcessStartInfo
            {
                FileName = HgPath,
                UseShellExecute = false,
                Arguments = string.Format("identify --repository \"{0}\"", _sourceDirectory),
                RedirectStandardOutput = true,

            };

            using (var exeProcess = Process.Start(identify))
            {
                exeProcess.WaitForExit();
                if (exeProcess.ExitCode > 0)
                {
                    throw new Exception("Error during get of latest revision name");
                }
                return exeProcess.StandardOutput.ReadLine();
            }
        }

        public void Revert()
        {

            var hgPurge = ProcessEx.GetProcess(HgPath, string.Format("purge --all --repository {0}", _sourceDirectory));
            using (var exeProcess = Process.Start(hgPurge))
            {
                var output = ProcessEx.GetOutput(exeProcess);
                if (exeProcess.ExitCode > 0)
                {
                    throw new Exception("Error during purge source code step" + output);
                }
            }
        }



        public void Purge()
        {
            var hgPurge = ProcessEx.GetProcess(HgPath, string.Format("purge --all --repository {0}", _sourceDirectory));
            using (var exeProcess = Process.Start(hgPurge))
            {
                var output = ProcessEx.GetOutput(exeProcess);
                if (exeProcess.ExitCode > 0)
                {
                    throw new Exception("Error during purge source code step" + output);
                }
            }
        }

        public void Update(string revisionNumber)
        {
            var hgUpdate = ProcessEx.GetProcess(HgPath, string.Format("update --repository {0} {1}", _sourceDirectory, GetRevisionString(revisionNumber)));
            using (var exeProcess = Process.Start(hgUpdate))
            {
                var output = ProcessEx.GetOutput(exeProcess);
                if (exeProcess.ExitCode > 0)
                {
                    throw new Exception("Error during updating to revision step" + output);
                }
            }
        }
    }
}
