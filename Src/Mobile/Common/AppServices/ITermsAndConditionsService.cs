using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface ITermsAndConditionsService
	{
        Task<TermsAndConditions> GetTerms();

		Task CheckIfNeedsToShowTerms (Action<object, Action<bool>> actionToDoIfTrue);

		void AcknowledgeTerms(bool acknowledged, string email);
	}
}

