using System.Collections.Generic;

namespace Cupertino.Model
{
    public class ProvisioningProfileList
    {
        public IEnumerable<ProvisioningProfile> ProvisioningProfiles { get; set; } 
    }

    public class ProvisioningProfile
    {
        public AppId AppId { get; set; }
        public string UUID { get; set; }
        public string ProvisioningProfileId { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public int DeviceCount { get; set; }

        public string DownloadUrl
        {
            get
            {
                return string.Format("https://developer.apple.com/account/ios/profile/profileContentDownload.action?displayId={0}", ProvisioningProfileId);
            }
        }

        public string EditUrl
        {
            get
            {
                return string.Format("https://developer.apple.com/account/ios/profile/profileEdit.action?provisioningProfileId={0}", ProvisioningProfileId);
            }
        }
    }

    public class AppId
    {
        public string Identifier { get; set; }
    }
}