namespace CustomerPortal.Web.Areas.Admin.Models
{
    public class CreateNewVersionRequest
    {
        public string CompanyKey { get; set; }
        public string WebsiteUrl { get; set; }
        public string VersionNumber { get; set; }
    }
}