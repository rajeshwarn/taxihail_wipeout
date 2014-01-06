using System;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Interfaces.Commands;
using apcurium.MK.Booking.Api.Contract.Resources;
using ServiceStack.Text;
using Cirrious.MvvmCross.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using System.Linq;

namespace apcurium.MK.Booking.Mobile
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
					if(AccountService.CurrentAccount.DefaultCreditCard == null)
					{
						AccountService.UpdateSettings(AccountService.CurrentAccount.Settings, PaymentPreferences.SelectedCreditCardId, AccountService.CurrentAccount.DefaultTipPercent);
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

