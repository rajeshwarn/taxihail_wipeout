using System;
using ServiceStack.ServiceHost;

namespace MK.Common.Android
{
    [Route("/vehicle/{CarNumber}")]
	public class SendMessageToDriverRequest : IReturn<SendMessageToDriverResponse>
	{
		public string Message {get; set;}

	    public string CarNumber {get; set;}
	}
}

