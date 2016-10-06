﻿#region

using System;
using System.IO;
using System.Diagnostics;

#endregion

namespace DeploymentServiceTools
{
    public class TaxiRepository
    {
        private readonly string _sourceDirectory;
        private readonly bool _isGitHub;
        private readonly bool _isNewFolder = false;

        public TaxiRepository(string sourceDirectory, bool isGitHub)
            : this(sourceDirectory, isGitHub, false)
        {
        }

        public TaxiRepository(string sourceDirectory, bool isGitHub, bool isNewFolder)
        {
            _sourceDirectory = sourceDirectory;
            _isGitHub = isGitHub;
            _isNewFolder = isNewFolder;
        }

        public void FetchSource(string revisionNumber, Action<string> logger)
        {
            var vsc = VersionControlToolsFactory.GetInstance(_sourceDirectory, _isGitHub);
            if (!Directory.Exists(_sourceDirectory) || _isNewFolder)
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
                        logger("Continuing...");
                    }
                    logger("Git Pull");
                    vsc.Pull();
                }
                catch (Exception ex)
                {
                    logger("Flow failed error :" + ex.Message);
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