using System;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Api.Contract.Requests;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class AccountConfirmationViewModel : PageViewModel
	{
		readonly IRegisterWorkflowService _registerService;
		RegisterAccount _account;

		public AccountConfirmationViewModel (IRegisterWorkflowService registerService)
		{
			_registerService = registerService;			
		}

		public void Init()
		{
			_account = _registerService.Account;
		}

		public string Code { get; set; }

		public ICommand ConfirmAccount 
		{
			get 
			{
				return this.GetCommand (async () => 
				{
					if(Code.HasValue())
					{
						try{
							_registerService.ConfirmAccount(Code);
							//all good
							_account.IsConfirmed = true;
							Close(this);
							_registerService.RegistrationFinished();

						}catch()
						{

						}
					}
				});
			}
		}
	}
}

