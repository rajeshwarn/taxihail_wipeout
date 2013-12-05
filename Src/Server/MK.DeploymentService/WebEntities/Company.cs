using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace CustomerPortal.Web.Entities
{
    public class Company
    {
        public Company()
        {
            IBS = new IBSSettings();
            Store = new StoreSettings();
            AppleAppStoreCredentials = new StoreCredentials();
            GooglePlayCredentials = new StoreCredentials();
            Versions = new List<Version>();
            CompanySettings = new List<CompanySetting>();
            
            LayoutRejected = new Dictionary<DateTime, string>();
        }


        public string Id { get; set; }
        public string CompanyKey { get; set; }
        public AppStatus Status { get; set; }

        [Required]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }

        [Display(Name = "Contact Name")]
        public string ProjectContactName { get; set; }

        [Display(Name = "Contact Email Address")]        
        public string ProjectContactEmail { get; set; }

        [Display(Name = "Contact Telephone")]
        public string ProjectContactTel { get; set; }

        [StringLength(4000)]
        [UIHint("MultilineText")]
        public string AppDescription { get; set; }


        public Dictionary<DateTime, string> LayoutRejected { get; set; }

        public IBSSettings IBS { get; set; }
        public StoreSettings Store { get; set; }

        public StoreCredentials AppleAppStoreCredentials { get; set; }

        public StoreCredentials GooglePlayCredentials { get; set; }

        public List<Version> Versions { get; set; }

        public Entities.Version FindVersion(string number)
        {
            return Versions.FirstOrDefault(x => x.Number == number);
        }
        public Entities.Version FindVersionById(string versionId)
        {
            return Versions.FirstOrDefault(x => x.VersionId == versionId);
        }

        public List<CompanySetting> CompanySettings { get; set; }

        [Obsolete]
        public Dictionary<string,string> GraphicsPaths { get; set; }

    }
    
}