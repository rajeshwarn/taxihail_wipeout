using System;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration.Impl;
using System.Threading.Tasks;
using apcurium.MK.Common.Extensions;
using MK.Common.Exceptions;

namespace apcurium.MK.Booking.Mobile.ViewModels.Payment
{
	public class CreditCardBaseViewModel : PageViewModel
	{
		private readonly ILocationService _locationService;
		private readonly IPaymentService _paymentService;
		private readonly IAccountService _accountService;


		private const int TipMaxPercent = 100;

		public CreditCardBaseViewModel(
			ILocationService locationService,
			IPaymentService paymentService, 
			IAccountService accountService)
		{
			_locationService = locationService;
			_paymentService = paymentService;
			_accountService = accountService;
		}

		public virtual void BaseOnViewStarted(bool firstTime)
		{
		}

		public override void OnViewStarted(bool firstTime)
		{
			base.OnViewStarted(firstTime);
			// we stop the service when the viewmodel starts because it stops after the homeviewmodel starts when we press back
			// this ensures that we don't stop the service just after having started it in homeviewmodel
			_locationService.Stop();

			BaseOnViewStarted(firstTime);
		}

		public ClientPaymentSettings PaymentSettings{ get; set;}


		public virtual void BaseStart()
		{
		}

		public override async void Start()
		{
			base.Start();
			try
			{
                PaymentSettings = await _paymentService.GetPaymentSettings();
			}
			catch(Exception ex)
			{
				Logger.LogError(ex);
			}

			BaseStart();
		}

		private PaymentDetailsViewModel _paymentPreferences;
		public PaymentDetailsViewModel PaymentPreferences
		{
			get
			{
				if (_paymentPreferences == null)
				{
					_paymentPreferences = Container.Resolve<PaymentDetailsViewModel>();
					_paymentPreferences.Start();
					_paymentPreferences.ActionOnTipSelected = SaveTip;
				}
				return _paymentPreferences;
			}
		}

		private ICommand SaveTip 
		{ 
			get
			{
				return this.GetCommand<int>(async tip =>
					{
						if (PaymentPreferences.Tip > TipMaxPercent)
						{
							await this.Services().Message.ShowMessage(null, this.Services().Localize["TipPercent_Error"]);
							return;
						}

						try
						{
							await _accountService.UpdateSettings(_accountService.CurrentAccount.Settings, _accountService.CurrentAccount.Email, PaymentPreferences.Tip);
						}
						catch (WebServiceException e)
						{
							Logger.LogError(e);
							this.Services()
								.Message.ShowMessage(this.Services().Localize["UpdateBookingSettingsInvalidDataTitle"],
									this.Services().Localize["UpdateBookingSettingsGenericError"]);
						}
					});
			} 
		} 

		public async Task GoToOverduePayment()
		{
			try
			{
				var overduePayment = await _paymentService.GetOverduePayment();

				if (overduePayment == null)
				{
					return;
				}

				this.Services().Message.ShowMessage(
					this.Services().Localize["View_Overdue"],
					this.Services().Localize["Overdue_OutstandingPaymentExists"],
					this.Services().Localize["OkButtonText"],
					() => ShowViewModelAndRemoveFromHistory<OverduePaymentViewModel>(new
						{
							overduePayment = overduePayment.ToJson()
						}),
					this.Services().Localize["Cancel"],
					() => Close(this));
			}
			catch (Exception ex)
			{
				Logger.LogError(ex);
			}
		}
	}
}

