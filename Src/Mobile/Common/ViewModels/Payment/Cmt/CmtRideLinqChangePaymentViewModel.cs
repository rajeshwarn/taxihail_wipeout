using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using ServiceStack.Text;
using System.Windows.Input;

namespace apcurium.MK.Booking.Mobile.ViewModels.Payment.Cmt
{
	public class CmtRideLinqChangePaymentViewModel : PageViewModel, ISubViewModel<PaymentInformation>
	{
		private readonly IAccountService _accountService;

		public CmtRideLinqChangePaymentViewModel(IAccountService accountService)
		{
			_accountService = accountService;			
		}

		public void Init(string currentPaymentInformation)
		{
			DefaultPaymentInformations = JsonSerializer.DeserializeFromString<PaymentInformation>(currentPaymentInformation);
			PaymentPreferences = Container.Resolve<PaymentDetailsViewModel>();
			PaymentPreferences.Start(DefaultPaymentInformations);
		}

		public PaymentInformation DefaultPaymentInformations { get; set ; }
		public PaymentDetailsViewModel PaymentPreferences { get; private set; }

		public ICommand CancelCommand
        {
            get
            {
				return this.GetCommand(() =>
				{
					this.ReturnResult(new PaymentInformation
					{
						CreditCardId = DefaultPaymentInformations.CreditCardId,
						TipPercent = DefaultPaymentInformations.TipPercent,
					});
				});
            }
        }

		public ICommand SaveCommand
        {
            get
            {
				return this.GetCommand(() =>
				{
					if (_accountService.CurrentAccount.DefaultCreditCard == null)
					{
						_accountService.UpdateSettings(_accountService.CurrentAccount.Settings, PaymentPreferences.SelectedCreditCardId, 
							_accountService.CurrentAccount.DefaultTipPercent);
					}

					this.ReturnResult(new PaymentInformation
					{
						CreditCardId = PaymentPreferences.SelectedCreditCardId,
						TipPercent = PaymentPreferences.Tip,
					});
				});
            }
        }
	}
}

