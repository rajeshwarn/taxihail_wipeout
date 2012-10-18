using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MK.ConfigurationManager.Entities
{
    public class DeploymentJob
    {
        [Key]
        public Guid Id { get; set; }

        public JobStatus Status { get; set; }

        public string Details { get; set; }

        public Company Company { get; set; }

        public IBSServer IBSServer { get; set; }

        public bool InitDatabase { get; set; }

        public bool Server { get; set; }
        public bool Android { get; set; }
        public bool iOS { get; set; }

        public TaxiHailEnvironment TaxHailEnv { get; set; }
    }

    public enum JobStatus   
    {
        REQUESTED,
        STARTED,
        INPROGRESS,
        SUCCESS,
        ERROR
    }
}