using ServiceStack.ServiceClient.Web;
using System.Net;

namespace MK.Booking.Api.Client
{
	public class TaxiHailJsonServiceClient : JsonServiceClient
	{
		public TaxiHailJsonServiceClient (string url) : base(url)
		{
			ServicePointManager.ServerCertificateValidationCallback = (o, c, c2, e) => {return true;};
		}
	}
}

