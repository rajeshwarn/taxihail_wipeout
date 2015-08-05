#region

using System;
using System.IO;
using System.Diagnostics;

#endregion

namespace DeploymentServiceTools
{
    public class TaxiRepository
    {
        private readonly string _exePath;
        private readonly string _sourceDirectory;
        private readonly bool _isGitHub;

        public TaxiRepository(string exePath, string sourceDirectory, bool isGitHub)
        {
            _exePath = exePath;
            _sourceDirectory = sourceDirectory;
            _isGitHub = isGitHub;
        }

        public void FetchSource(string revisionNumber, Action<string> logger)
        {
            var vsc = VersionControlToolsFactory.GetInstance(_exePath, _sourceDirectory, _isGitHub);
            if (!Directory.Exists(_sourceDirectory))
            {
                logger("Full Clone");
                vsc.Clone(revisionNumber);
            }
            else
            {
                try
                {
					logger("Git Revert");
                    vsc.Revert();
                    logger("Git Purge");
                    try
                    {
                        vsc.Purge();
                    }
                    catch (Exception e)
                    {
                        logger("PurgeFailed: " + e.Message);
                        logger("Continuing...);
                    }
                    logger("Git Pull");
                    vsc.Pull();
                }
                catch (Exception)
                {
                    logger("Revert Failed - Deleting all files");
                    Delete();

                    logger("Full Clone");
                    vsc.Clone(revisionNumber);
                }
            }

            logger("Git Update to rev " + revisionNumber);
            vsc.Update(revisionNumber);
        }

        private void Delete()
        {
            var delete = ProcessEx.IsMac() 
                ? ProcessEx.GetProcess("rm", string.Format("-r -f {0}", _sourceDirectory)) 
                : ProcessEx.GetProcess("rmdir", string.Format("/s /q {0}", _sourceDirectory));

            using (var exeProcess = Process.Start(delete))
            {
                var output = ProcessEx.GetOutput(exeProcess);
                if (exeProcess.ExitCode > 0)
                {
                    throw new Exception("Error during delete sourceFolder" + output);
                }
            }
        }
    }
}