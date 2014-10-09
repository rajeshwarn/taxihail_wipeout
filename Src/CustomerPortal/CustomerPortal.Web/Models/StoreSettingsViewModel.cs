#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CustomerPortal.Web.Entities;
using CustomerPortal.Web.Properties;

#endregion

namespace CustomerPortal.Web.Models
{
    public class StoreSettingsViewModel : StoreSettings
    {
        public override List<string> UniqueDeviceIdentificationNumber
        {
            get
            {
                return new[]
                {
                    UniqueDeviceIdentificationNumber0,
                    UniqueDeviceIdentificationNumber1,
                    UniqueDeviceIdentificationNumber2,
                    UniqueDeviceIdentificationNumber3,
                    UniqueDeviceIdentificationNumber4,
                    UniqueDeviceIdentificationNumber5,
                    UniqueDeviceIdentificationNumber6,
                    UniqueDeviceIdentificationNumber7,
                    UniqueDeviceIdentificationNumber8,
                    UniqueDeviceIdentificationNumber9
                }.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            }
        }

        [Display(Name = "Keywords", Description = "KeywordsHelp", ResourceType = typeof (Resources))]
        [StringLength(92)]
        [UIHint("MultilineText")]
        public string Keywords { get; set; }

        [Display(Name = "UniqueDeviceIdentificationNumber", Description = "UniqueDeviceIdentificationNumberHelp",
            ResourceType = typeof (Resources))]
        [RegularExpression("[a-fA-F0-9]{40}", ErrorMessageResourceName = "UniqueDeviceIdentificationNumberError",
            ErrorMessageResourceType = typeof (Resources))]
        public string UniqueDeviceIdentificationNumber0 { get; set; }

        [RegularExpression("[a-fA-F0-9]{40}", ErrorMessageResourceName = "UniqueDeviceIdentificationNumberError",
            ErrorMessageResourceType = typeof (Resources))]
        public string UniqueDeviceIdentificationNumber1 { get; set; }

        [RegularExpression("[a-fA-F0-9]{40}", ErrorMessageResourceName = "UniqueDeviceIdentificationNumberError",
            ErrorMessageResourceType = typeof (Resources))]
        public string UniqueDeviceIdentificationNumber2 { get; set; }

        [RegularExpression("[a-fA-F0-9]{40}", ErrorMessageResourceName = "UniqueDeviceIdentificationNumberError",
            ErrorMessageResourceType = typeof (Resources))]
        public string UniqueDeviceIdentificationNumber3 { get; set; }

        [RegularExpression("[a-fA-F0-9]{40}", ErrorMessageResourceName = "UniqueDeviceIdentificationNumberError",
            ErrorMessageResourceType = typeof (Resources))]
        public string UniqueDeviceIdentificationNumber4 { get; set; }

        [RegularExpression("[a-fA-F0-9]{40}", ErrorMessageResourceName = "UniqueDeviceIdentificationNumberError",
            ErrorMessageResourceType = typeof (Resources))]
        public string UniqueDeviceIdentificationNumber5 { get; set; }

        [RegularExpression("[a-fA-F0-9]{40}", ErrorMessageResourceName = "UniqueDeviceIdentificationNumberError",
            ErrorMessageResourceType = typeof (Resources))]
        public string UniqueDeviceIdentificationNumber6 { get; set; }

        [RegularExpression("[a-fA-F0-9]{40}", ErrorMessageResourceName = "UniqueDeviceIdentificationNumberError",
            ErrorMessageResourceType = typeof (Resources))]
        public string UniqueDeviceIdentificationNumber7 { get; set; }

        [RegularExpression("[a-fA-F0-9]{40}", ErrorMessageResourceName = "UniqueDeviceIdentificationNumberError",
            ErrorMessageResourceType = typeof (Resources))]
        public string UniqueDeviceIdentificationNumber8 { get; set; }

        [RegularExpression("[a-fA-F0-9]{40}", ErrorMessageResourceName = "UniqueDeviceIdentificationNumberError",
            ErrorMessageResourceType = typeof (Resources))]
        public string UniqueDeviceIdentificationNumber9 { get; set; }

        public AndroidStoreCredentials GooglePlayCredentials { get; set; }
        public AppleStoreCredentials AppStoreCredentials { get; set; }

        public static object CreateFrom(StoreSettings model, AppleStoreCredentials appStoreCredentials,
            AndroidStoreCredentials googlePlayCredentials)
        {
            return new StoreSettingsViewModel
            {
                Keywords = model.Keywords,
                UniqueDeviceIdentificationNumber0 = model.UniqueDeviceIdentificationNumber.Skip(0).FirstOrDefault(),
                UniqueDeviceIdentificationNumber1 = model.UniqueDeviceIdentificationNumber.Skip(1).FirstOrDefault(),
                UniqueDeviceIdentificationNumber2 = model.UniqueDeviceIdentificationNumber.Skip(2).FirstOrDefault(),
                UniqueDeviceIdentificationNumber3 = model.UniqueDeviceIdentificationNumber.Skip(3).FirstOrDefault(),
                UniqueDeviceIdentificationNumber4 = model.UniqueDeviceIdentificationNumber.Skip(4).FirstOrDefault(),
                UniqueDeviceIdentificationNumber5 = model.UniqueDeviceIdentificationNumber.Skip(5).FirstOrDefault(),
                UniqueDeviceIdentificationNumber6 = model.UniqueDeviceIdentificationNumber.Skip(6).FirstOrDefault(),
                UniqueDeviceIdentificationNumber7 = model.UniqueDeviceIdentificationNumber.Skip(7).FirstOrDefault(),
                UniqueDeviceIdentificationNumber8 = model.UniqueDeviceIdentificationNumber.Skip(8).FirstOrDefault(),
                UniqueDeviceIdentificationNumber9 = model.UniqueDeviceIdentificationNumber.Skip(9).FirstOrDefault(),
                AppStoreCredentials = appStoreCredentials,
                GooglePlayCredentials = googlePlayCredentials,
            };
        }
    }
}