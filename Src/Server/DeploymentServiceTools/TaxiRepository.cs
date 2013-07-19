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
                try{
					hg.Purge();
				}
				catch(Exception e)
				{
					logger ("PurgeFailed: " + e.Message);
				}
                logger("Hg Pull");
                hg.Pull();
            }
			
			logger("Hg Update to rev "+ revisionNumber);
            hg.Update(revisionNumber);
        }
    }
}
