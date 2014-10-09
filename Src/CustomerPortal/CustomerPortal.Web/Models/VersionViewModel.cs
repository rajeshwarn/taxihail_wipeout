#region

using System;
using CustomerPortal.Web.Entities;
using Version = CustomerPortal.Web.Entities.Version;

#endregion

namespace CustomerPortal.Web.Models
{
    public class VersionViewModel
    {
        public string VersionId { get; set; }
        public string Number { get; set; }
        public string ApplicationName { get; set; }
        public string CompanyName { get; set; }
        public string CompanyId { get; set; }
        public string ApkFilename { get; set; }
        public string IpaFilename { get; set; }

        public string IpaAppStoreFilename { get; set; }
        
        public string WebsiteUrl { get; set; }
        public DateTime CreatedOn { get; set; }

        public bool HasIpa
        {
            get { return IpaFilename != null; }
        }

        public bool HasIpaAppStore
        {
            get { return IpaAppStoreFilename != null; }
        }


        public bool HasApk
        {
            get { return ApkFilename != null; }
        }

        public bool HasWebsite
        {
            get { return !string.IsNullOrEmpty(WebsiteUrl); }
        }

        public static VersionViewModel CreateFrom(Company company, Version version)
        {
            return new VersionViewModel
            {
                CompanyName = company.CompanyName,
                CompanyId = company.Id,
                ApplicationName = company.Application.AppName ?? company.CompanyName,
                ApkFilename = version.ApkFilename,
                IpaFilename = version.IpaFilename,
                IpaAppStoreFilename = version.IpaAppStoreFilename,
                WebsiteUrl = version.WebsiteUrl,
                VersionId = version.VersionId,
                Number = version.Number,
                CreatedOn = version.CreatedOn,
            };
        }
    }
}