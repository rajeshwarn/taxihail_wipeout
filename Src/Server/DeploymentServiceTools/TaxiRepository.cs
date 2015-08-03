#region

using System;
using System.IO;
using System.Diagnostics;

#endregion

namespace DeploymentServiceTools
{
    public class TaxiRepository
    {
        private readonly string _gitPath;
        private readonly string _sourceDirectory;

        public TaxiRepository(string gitPath, string sourceDirectory)
        {
            _gitPath = gitPath;
            _sourceDirectory = sourceDirectory;
        }

        public void FetchSource(string revisionNumber, Action<string> logger)
        {
            var git = new GitTools(_gitPath, _sourceDirectory);

            if (!Directory.Exists(_sourceDirectory))
            {
                logger("Full Clone");
                git.Clone(revisionNumber);
            }
            else
            {
                try
                {
					logger("Git Revert");
                    git.Revert();
                    logger("Git Purge");
                    try
                    {
                        git.Purge();
                    }
                    catch (Exception e)
                    {
                        logger("PurgeFailed: " + e.Message);
                    }
                    logger("Git Pull");
                    git.Pull();
                }
                catch (Exception)
                {
                    logger("Revert Failed - Deleting all files");
                    Delete();

                    logger("Full Clone");
                    git.Clone(revisionNumber);
                }
            }

            logger("Git Update to rev " + revisionNumber);
            git.Update(revisionNumber);
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