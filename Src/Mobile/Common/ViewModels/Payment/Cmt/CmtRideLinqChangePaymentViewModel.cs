using System;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using ServiceStack.Text;

namespace apcurium.MK.Booking.Mobile.ViewModels.Payment.Cmt
{
	public class CmtRideLinqChangePaymentViewModel : BaseSubViewModel<PaymentInformation>, IMvxServiceConsumer<IAccountService>
	{
		public CmtRideLinqChangePaymentViewModel(string messageId, string currentPaymentInformation): base(messageId)
		{
			DefaultPaymentInformations = JsonSerializer.DeserializeFromString<PaymentInformation>(currentPaymentInformation);
			PaymentPreferences = new PaymentDetailsViewModel(Guid.NewGuid().ToString(), DefaultPaymentInformations);
			PaymentPreferences.LoadCreditCards();
		}

		public PaymentInformation DefaultPaymentInformations { get; set ; }
		public PaymentDetailsViewModel PaymentPreferences { get; private set; }

		public IMvxCommand CancelCommand
        {
            get
            {
				return GetCommand(() =>
				{
					ReturnResult(new PaymentInformation
					{
						CreditCardId = DefaultPaymentInformations.CreditCardId,
						TipPercent = DefaultPaymentInformations.TipPercent,
					});
				});
            }
        }

		public IMvxCommand SaveCommand
        {
            get
            {
				return GetCommand(() =>
				{
                    if (this.Services().Account.CurrentAccount.DefaultCreditCard == null)
					{
                        this.Services().Account.UpdateSettings(this.Services().Account.CurrentAccount.Settings, PaymentPreferences.SelectedCreditCardId, 
                                    this.Services().Account.CurrentAccount.DefaultTipPercent);
					}

					ReturnResult(new PaymentInformation
					{
						CreditCardId = PaymentPreferences.SelectedCreditCardId,
						TipPercent = PaymentPreferences.Tip,
					});
				});
            }
        }
	}
}

