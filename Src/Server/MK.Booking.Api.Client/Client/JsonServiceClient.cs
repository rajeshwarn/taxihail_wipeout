using System;
using ServiceStack.ServiceClient.Web;
using System.Net;

namespace MK.Booking.Api.Client.iOS.Client
{
	public class TaxihailJsonServiceClient : JsonServiceClient
	{
		public TaxihailJsonServiceClient (string url) : base(url)
		{
			ServicePointManager.ServerCertificateValidationCallback = (o, c, c2, e) => {return true;};
		}
	}
}

