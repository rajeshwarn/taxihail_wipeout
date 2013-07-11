using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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

        public  void FetchSource(string revisionNumber, Action<string> logger)
        {
            var hg = new MecurialTools(_hgPath, _sourceDirectory);

            
            if (string.IsNullOrEmpty(revisionNumber))
            {
                logger("No revision or version specified - Using tip");
                revisionNumber = hg.GetTipRevisionNumber();
            }

            if (!Directory.Exists(_sourceDirectory))
            {
                logger("Full Clone");
                hg.Clone(revisionNumber);
            }
            else
            {
                logger("Hg Revert");
                hg.Revert();
                logger("Hg Purge");
                hg.Purge();
                logger("Hg Pull");
                hg.Pull();
            }

            hg.Update(revisionNumber);

        }
    }
}
