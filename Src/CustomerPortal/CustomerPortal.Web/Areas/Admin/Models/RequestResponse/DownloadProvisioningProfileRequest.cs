namespace CustomerPortal.Web.Areas.Admin.Models
{
    public class DownloadProvisioningProfileRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Team { get; set; }
        public string AppId { get; set; }
        public bool AdHoc { get; set; }
    }
}