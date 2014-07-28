using System;
using apcurium.MK.Booking.Api.Contract.Requests;
using System.Threading.Tasks;
using System.Reactive.Subjects;
using apcurium.MK.Booking.Api.Client;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{

	public class RegisterWorkflowService : IRegisterWorkflowService
	{
		readonly IAccountService _accountService;
		readonly ISubject<RegisterAccount> _registrationAddressSubject = new BehaviorSubject<RegisterAccount>(null);
		RegisterAccount _account;
		IAccountServiceClient _accountServiceClient;

		public RegisterWorkflowService (IAccountService accountService, IAccountServiceClient accountServiceClient)
		{
			_accountServiceClient = accountServiceClient;
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

		public async Task ConfirmAccount (string code)
		{
			await _accountServiceClient.ConfirmAccount (new ConfirmAccountRequest 
			{
				ConfirmationToken = code,
				EmailAddress = _account.Email,
				IsSMSConfirmation = true
			});
		}

		public void RegistrationFinished ()
		{
			_registrationAddressSubject.OnNext (_account);
		}

		public RegisterAccount Account {
			get 
			{
				return _account;
			}
		}
	}
}
