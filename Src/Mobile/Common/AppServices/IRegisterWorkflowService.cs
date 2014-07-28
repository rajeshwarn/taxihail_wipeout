using System;
using apcurium.MK.Booking.Api.Contract.Requests;
using System.Threading.Tasks;
using System.Reactive.Subjects;
using apcurium.MK.Booking.Api.Client;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface IRegisterWorkflowService
	{
		void BeginRegistration ();
		IObservable<RegisterAccount> GetAndObserveRegistration();
		Task RegisterAccount (RegisterAccount data);
		Task ConfirmAccount (string code);
		void RegistrationFinished();
		RegisterAccount Account { get; }
	}
}
