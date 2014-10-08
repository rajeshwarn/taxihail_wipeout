namespace CustomerPortal.Web.Entities.Network
{
    public class CompanyPreference
    {
        public string CompanyId { get; set; }
        public bool CanAccept { get; set; }
        public bool CanDispatch { get; set; }
    }
}