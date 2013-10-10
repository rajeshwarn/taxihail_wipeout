using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MK.ConfigurationManager.Entities
{
    public class DeploymentJob
    {
        [Key]
        public Guid Id { get; set; }

        public JobStatus Status { get; set; }

        public string Details { get; set; }

        public string Revision { get; set; }

        public Company Company { get; set; }

        public IBSServer IBSServer { get; set; }
       
        public bool DeployServer { get; set; }
        public bool DeployDB { get; set; }
        public bool InitDatabase { get; set; }
        public bool Android { get; set; }
		public bool CallBox { get;set; }
        public bool iOS_AdHoc { get; set; }
		public bool iOS_AppStore { get;set; }

        public TaxiHailEnvironment TaxHailEnv { get; set; }


        /**use by peca poco on mono */
		[NotMapped]
		public Guid Company_Id { get; set; }
		[NotMapped]
		public Guid IBSServer_Id { get; set; }
		[NotMapped]
		public Guid TaxHailEnv_Id { get; set; }
        [NotMapped]
        public Guid Version_Id { get; set; }

        public DateTime? RequestedDate { get; set; }

        public AppVersion Version { get; set; }

        public string GetRevisionNumber()
        {
            if (Version != null)
            {
                return Version.Revision;
            }

            return string.IsNullOrEmpty(Revision) ? string.Empty : Revision;
        }

        public string GetVersionNumber()
        {
           return Version != null ? Version.Display : string.Empty;
        }

    }

    public enum JobStatus   
    {
        REQUESTED,
        INPROGRESS,
        SUCCESS,
        ERROR
    }
}