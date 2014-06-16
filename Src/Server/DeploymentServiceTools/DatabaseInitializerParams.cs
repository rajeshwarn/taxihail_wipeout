using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeploymentServiceTools
{
    public class DatabaseInitializerParams
    {
        public DatabaseInitializerParams()
        {
        
        }

        public string CompanyName { get; set; }

        public string MkWebConnectionString { get; set; }

        public string MasterConnectionString { get; set; }

        public string SqlInstanceName { get; set; }        
        
        public string BackupFolder { get; set; }

        public string MirroringSharedFolder { get; set; }

        public string MirrorMasterConnectionString { get; set; }

        public string MirroringMirrorPartner { get; set; }
        
        public string MirroringPrincipalPartner { get; set; }
        public string MirroringWitness { get; set; }

        
        
    }
}
