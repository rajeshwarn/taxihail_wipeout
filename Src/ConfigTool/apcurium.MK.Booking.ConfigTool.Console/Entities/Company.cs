using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CustomerPortal.Web.Properties;
using MongoDB.Bson.Serialization.Attributes;
using MongoRepository;

namespace CustomerPortal.Web.Entities
{
    public class Company: IEntity
    {
        public Company()
        {
            AppDescription = Resources.SampleAppDescription;
            IBS = new IBSSettings();
            Application = new Questionnaire();
            Store = new StoreSettings();
            AppleAppStoreCredentials = new StoreCredentials();
            GooglePlayCredentials = new StoreCredentials();
            Versions = new List<Version>();
            Settings = new Dictionary<string, string>();
            GraphicsPaths = new Dictionary<string, string>();
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
        [EmailAddress]
        public string ProjectContactEmail { get; set; }

        [Display(Name = "Contact Telephone")]
        public string ProjectContactTel { get; set; }

        [Display(Name = "AppDescription", Description = "AppDescriptionHelp", ResourceType = typeof(Resources))]
        [StringLength(4000)]
        [UIHint("MultilineText")]
        public string AppDescription { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? LayoutsApprovedOn { get; set; }

        public Dictionary<DateTime, string> LayoutRejected { get; set; }

        public IBSSettings IBS { get; set; }
        public Questionnaire Application { get; set; }
        public StoreSettings Store { get; set; }

        [Display(Name = "AppleAppStoreCredentials", Description = "AppleAppStoreCredentialsHelp", ResourceType = typeof(Resources))]
        public StoreCredentials AppleAppStoreCredentials { get; set; }

        [Display(Name = "GooglePlayCredentials", Description = "GooglePlayCredentialsHelp", ResourceType = typeof(Resources))]
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
        public Dictionary<string, string> Settings { get; set; }

        public Dictionary<string,string> GraphicsPaths { get; set; }

    }
    
}