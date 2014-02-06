#region

using System;
using System.IO;
using System.Diagnostics;

#endregion

namespace DeploymentServiceTools
{
    public class TaxiRepository
    {
        private readonly string _hgPath;
        private readonly string _sourceDirectory;

        public TaxiRepository(string hgPath, string sourceDirectory)
        {
            _hgPath = hgPath;
            _sourceDirectory = sourceDirectory;
        }

        public void FetchSource(string revisionNumber, Action<string> logger)
        {
            var hg = new MecurialTools(_hgPath, _sourceDirectory);

            if (!Directory.Exists(_sourceDirectory))
            {
                logger("Full Clone");
                hg.Clone(revisionNumber);
            }
            else
            {
                try
                {
					logger("Hg Revert");
                    hg.Revert();
                    logger("Hg Purge");
                    try
                    {
                        hg.Purge();
                    }
                    catch (Exception e)
                    {
                        logger("PurgeFailed: " + e.Message);
                    }
                    logger("Hg Pull");
                    hg.Pull();
                }
                catch (Exception)
                {
                    logger("Revert Failed - Deleting all files");

					var delete = ProcessEx.GetProcess("rm",string.Format("-r -f {0}", _sourceDirectory));
					using (var exeProcess = Process.Start(delete))
					{
						var output = ProcessEx.GetOutput(exeProcess);
						if (exeProcess.ExitCode > 0)
						{
							throw new Exception("Error during delete sourceFolder" + output);
						}
					}

                    logger("Full Clone");
                    hg.Clone(revisionNumber);
                }
            }

            logger("Hg Update to rev " + revisionNumber);
            hg.Update(revisionNumber);
        }
    }
}