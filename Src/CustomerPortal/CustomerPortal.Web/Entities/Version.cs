#region

using System;
using System.ComponentModel.DataAnnotations;

#endregion

namespace CustomerPortal.Web.Entities
{
    public class Version
    {
        public string VersionId { get; set; }

        [Required]
        public string Number { get; set; }

        [Display(Name = "iOS Adhoc Package File (.ipa)")]
        public string IpaFilename { get; set; }

        [Display(Name = "iOS App Store Package File (.ipa)")]
        public string IpaAppStoreFilename { get; set; }
        
        [Display(Name = "Android Application Package File (.apk)")]
        public string ApkFilename { get; set; }

        [Display(Name = "BlackBerry Application Package File (.apk)")]
        public string ApkBlackBerryFilename { get; set; }

        [Display(Name = "BlackBerry Archive File (.bar)")]
        public string BarFilename { get; set; }

        [Display(Name = "Callbox application package file (.apk)")]
        public string ApkCallboxFileName { get; set; }

        [Display(Name = "Website Url")]
        [UIHint("Url")]
        public string WebsiteUrl { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}