using System;
using apcurium.MK.Booking.Api.Contract.Requests;
using System.Threading.Tasks;
using System.Reactive.Subjects;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Common;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface IRegisterWorkflowService
	{
		void PrepareNewRegistration ();
		IObservable<RegisterAccount> GetAndObserveRegistration();
		Task RegisterAccount (RegisterAccount data);
		Task ConfirmAccount(string code);

        Task GetConfirmationCode(CountryISOCode countryCode, string phoneNumber);

        void RegistrationFinished();
        RegisterAccount Account { get; set; }
	}
}
