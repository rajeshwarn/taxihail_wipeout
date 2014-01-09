using System;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using ServiceStack.Text;
using System.Windows.Input;

namespace apcurium.MK.Booking.Mobile.ViewModels.Payment.Cmt
{
	public class CmtRideLinqChangePaymentViewModel : BaseSubViewModel<PaymentInformation>
	{
		public CmtRideLinqChangePaymentViewModel(string messageId, string currentPaymentInformation): base(messageId)
		{
			DefaultPaymentInformations = JsonSerializer.DeserializeFromString<PaymentInformation>(currentPaymentInformation);
			PaymentPreferences = new PaymentDetailsViewModel(Guid.NewGuid().ToString(), DefaultPaymentInformations);
			PaymentPreferences.LoadCreditCards();
		}

		public PaymentInformation DefaultPaymentInformations { get; set ; }
		public PaymentDetailsViewModel PaymentPreferences { get; private set; }

		public ICommand CancelCommand
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

		public ICommand SaveCommand
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

