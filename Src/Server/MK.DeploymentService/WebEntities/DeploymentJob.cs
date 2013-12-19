using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CustomerPortal.Web.Entities
{
    public class DeploymentJob
    {
        public string Id { get; set; }
        [Display(Name = "Company")]
        public Company Company { get; set; }
        public string Status { get; set; }
        [Display(Name = "Server")]
        public Environment Server { get; set; }
        [Display(Name = "Revision")]
        public Revision Revision { get; set; }
        public bool ServerSide { get; set; }
        [Display(Name = "Database")]
        public bool Database { get; set; }
        public bool ClientSide { get; set; }
        public bool Android { get; set; }
        public bool CallBox { get; set; }
        public bool IosAdhoc { get; set; }
        public bool IosAppStore { get; set; }
        public DateTime Date { get; set; }
        public string Details { get; set; }

		public string ServerUrl {
			get;
			set;
		}

        public DeploymentJob()
        {
            ServerSide = true;
            Database = false;
            ClientSide = false;
            Android = false;
            CallBox = false;
            IosAdhoc = false;
            Status = "Requested";
        }
    }

    public enum JobStatus
    {
        Requested,
        Inprogress,
        Success,
        Error
    }
}