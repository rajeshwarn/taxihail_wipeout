using System;
using apcurium.MK.Booking.Api.Contract.Requests;
using System.Threading.Tasks;
using System.Reactive.Subjects;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface IRegisterWorkflowService
	{
		void BeginRegistration ();
		IObservable<RegisterAccount> GetAndObserveRegistration();
		Task RegisterAccount (RegisterAccount data);
		void RegistrationFinished();
	}

	public class RegisterWorkflowService : IRegisterWorkflowService
	{
		readonly IAccountService _accountService;
		readonly ISubject<RegisterAccount> _registrationAddressSubject = new BehaviorSubject<RegisterAccount>(new RegisterAccount());
		RegisterAccount _account;

		public RegisterWorkflowService (IAccountService accountService)
		{
			_accountService = accountService;			
		}

		public void BeginRegistration ()
		{
			_account = null;
		}

		public IObservable<RegisterAccount> GetAndObserveRegistration ()
		{
			return _registrationAddressSubject;
		}

		public async Task RegisterAccount (RegisterAccount data)
		{
			await _accountService.Register(data);
			_account = data;
		}

		public void RegistrationFinished ()
		{
			_registrationAddressSubject.OnNext (_account);
		}
	}
}

