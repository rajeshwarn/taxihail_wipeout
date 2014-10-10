namespace CustomerPortal.Contract.Resources
{
    public class CompanyPreference
    {
        public string CompanyId { get; set; }
        public bool CanAccept { get; set; }
        public bool CanDispatch { get; set; }
    }
}