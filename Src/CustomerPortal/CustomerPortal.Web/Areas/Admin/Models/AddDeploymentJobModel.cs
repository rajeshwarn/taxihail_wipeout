﻿#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CustomerPortal.Web.Entities;

#endregion

namespace CustomerPortal.Web.Areas.Admin.Models
{
    public enum ServerUrlOptions
    {
        Staging = 1,
        Production = 2,
        Dev = 3,
        Other = 4,
        Arro = 5,
    }

    public class AddDeploymentJobModel
    {
        public AddDeploymentJobModel()
        {
            ModelForView = new DeploymentModel();
            SelectedCompaniesId = new List<string>();
        }

        public string Title { get; set; }

        public DeploymentModel ModelForView { get; set; }

        public int CreateType { get; set; }

        public DeploymentJobType Type
        {
            get { return (DeploymentJobType) CreateType; }
        }

        [Required]
        [Display(Name = "Revision")]
        public string RevisionId { get; set; }

        [Required]
        [Display(Name = "Server")]
        public string ServerId { get; set; }

        [Required]
        [Display(Name = "Company")]
        public string CompanyId { get; set; }

        public List<string> SelectedCompaniesId { get; set; }

        [Display(Name = "ServerUrl")]
        public string ServerUrl { get; set; }

        public ServerUrlOptions ServerUrlOptions { get; set; }

        [Display(Name = "Deploy Database")]
        public bool Database { get; set; }
        public bool Android { get; set; }
        public bool CallBox { get; set; }
        public bool IosAdhoc { get; set; }
        public bool IosAppStore { get; set; }
        public bool ShowCompanyDetails { get; set; }
    }
}