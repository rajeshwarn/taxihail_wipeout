#region

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Odbc;
using System.Linq;
using CustomerPortal.Web.Entities.Network;
using CustomerPortal.Web.Properties;
using MongoDB.Bson.Serialization.Attributes;
using MongoRepository;

#endregion

namespace CustomerPortal.Web.Entities
{
    [BsonIgnoreExtraElements]
    public class Company : IEntity
    {
        public Company()
        {
            AppDescription = Resources.SampleAppDescription;
            IBS = new IBSSettings();
            Application = new Questionnaire();
            Store = new StoreSettings();
            Network = new TaxiHailNetworkSettings();

            AppleAppStoreCredentials = new AppleStoreCredentials();
            GooglePlayCredentials = new AndroidStoreCredentials();
            Versions = new List<Version>();
            CompanySettings = new List<CompanySetting>();
            Style = new Style();
            Payment = new Payment();            
            LayoutRejected = new Dictionary<DateTime, string>();
            Errors = new Dictionary<string, string>();
        }

        public Style Style { get; set; }
        public Payment Payment { get; set; }
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

        [Display(Name = "AppDescription", Description = "AppDescriptionHelp", ResourceType = typeof (Resources))]
        [StringLength(4000)]
        [UIHint("MultilineText")]
        public string AppDescription { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? LayoutsApprovedOn { get; set; }

        public Dictionary<DateTime, string> LayoutRejected { get; set; }

        public IBSSettings IBS { get; set; }
        public Questionnaire Application { get; set; }
        public StoreSettings Store { get; set; }

        [Display(Name = "TaxiHailNetworkSettings",ResourceType = typeof(Resources))]
        public TaxiHailNetworkSettings Network { get; set; }

        [Display(Name = "AppleAppStoreCredentials", Description = "AppleAppStoreCredentialsHelp",
            ResourceType = typeof (Resources))]
        public AppleStoreCredentials AppleAppStoreCredentials { get; set; }

        [Display(Name = "GooglePlayCredentials", Description = "GooglePlayCredentialsHelp",
            ResourceType = typeof (Resources))]
        public AndroidStoreCredentials GooglePlayCredentials { get; set; }

        public List<Version> Versions { get; set; }
        public List<CompanySetting> CompanySettings { get; set; }
        public string LastKnownProductionVersion { get; set; }


        public string LastKnownStagingVersion { get; set; }


        public Dictionary<string, string> Errors { get; set; }
        public string Id { get; set; }

        public Version FindVersion(string number)
        {
            return Versions.FirstOrDefault(x => x.Number == number);
        }

        public Version FindVersionById(string versionId)
        {
            return Versions.FirstOrDefault(x => x.VersionId == versionId);
        }

        public bool IsValid()
        {
            return ((IBS != null) && IBS.LastSucessfullTestDateTime.HasValue && (Errors.Count == 0));
        }

       
        public string ValidationError()
        {
            string result = "";
            if ((IBS == null) || !IBS.LastSucessfullTestDateTime.HasValue)
            {
                result = @"IBS Server was never tested<br/>";
            }

            if (Errors.Any())
            {
                foreach (var err in Errors.Values)
                {
                    result += err + @"<br/>";
                }
            }
            return result;
        }


        
      


        public bool IsStoreValid()
        {
            return Errors.Keys.Count(k => k.Contains("iOSError")) == 0;
        }

        public string StoreValidationError()
        {
            string result = "";

            var iosErrors = Errors.Where(x => x.Key.Contains("iOSError")).ToDictionary(x => x.Key, x => x.Value);
            if (iosErrors.Any())
            {
                foreach (var err in iosErrors.Values)
                {
                    result += err + @"<br/>";
                }
            }
            return result;
        }
    }
}