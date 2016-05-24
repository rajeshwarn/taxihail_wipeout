using System;
using apcurium.MK.Common.Resources;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
<<<<<<< HEAD
	[Authenticate]
	[Route("/account/creditcard/updatevalidationdate", "POST")]
	public class UpdateCreditCardValidationDateRequest : IReturn<BasePaymentResponse>
	{
		public Guid CreditCardId { get; set; }
		//public DateTime? LastTokenValidateDateTime { get; set; }
		//public int Year { get; set; }
		public DateTime? LastTokenValidateDateTime { get; set; }
		//public DateTime LastTokenValidateDateTimeNotNull { get; set; }
	}
}




=======
    [Authenticate]
    [Route("/account/creditcard/updatevalidationdate", "POST")]
    public class UpdateCreditCardValidationDateRequest : IReturn<BasePaymentResponse>
    {
        public Guid CreditCardId { get; set; }
        public DateTime? LastTokenValidateDateTime { get; set; }

    }
}
>>>>>>> 617bd1c7fe7996c834f397f04d189ef1282dbe1e
