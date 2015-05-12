
namespace CustomerPortal.Contract.Response
{
    public class NetworkFleetResponse
    {
        public string CompanyName { get; set; }

        public string CompanyKey { get; set; }

        public int FleetId { get; set; }

        public string RestApiUrl  { get; set; }
        
        public string RestApiUser  { get; set; }
        
        public string RestApiSecret { get; set; }
        
        public string IbsUrl { get; set; }

        public string IbsUserName { get; set; }

        public string IbsPassword { get; set; }

        public long IbsTimeDifference { get; set; }
    }
}
