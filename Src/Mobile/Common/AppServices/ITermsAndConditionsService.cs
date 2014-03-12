using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.PresentationHints;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface ITermsAndConditionsService
	{
        Task<TermsAndConditions> GetTerms();

		Task CheckIfNeedsToShowTerms (Action<object, Action<bool>> actionToDoIfTrue, Action<bool, ZoomToStreetLevelPresentationHint> actionToDoOnReturn, bool initialLocateUserValue, ZoomToStreetLevelPresentationHint initialHintValue);

		void AcknowledgeTerms(bool acknowledged, string email);
	}
}

