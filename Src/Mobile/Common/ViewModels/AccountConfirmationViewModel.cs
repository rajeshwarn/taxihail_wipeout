using System;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Api.Contract.Requests;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Extensions;
using ServiceStack.ServiceClient.Web;

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
							await _registerService.ConfirmAccount(Code);
							_account.IsConfirmed = true;
							Close(this);
							_registerService.RegistrationFinished();

						}catch(WebServiceException e)
						{
							var errorMessage = this.Services().Localize["ServiceError" + e.Message];
							if(errorMessage == "ServiceError" + e.Message)
							{
								errorMessage = e.Message;
							}

							this.Services().Message.ShowMessage(this.Services().Localize["AccountConfirmation_ErrorTitle"], errorMessage);
						}
					}
				});
			}
		}
	}
}

