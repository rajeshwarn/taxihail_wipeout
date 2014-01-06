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
        private readonly IAccountService _accountService;
        public CmtRideLinqChangePaymentViewModel(string messageId, string order, string orderStatus): base(messageId)
		{
			_accountService  = this.GetService<IAccountService>();

			var account = AccountService.CurrentAccount;
			var paymentInformation = new PaymentInformation
			{
				CreditCardId = account.DefaultCreditCard,
				TipPercent = account.DefaultTipPercent,
			};


			DefaultPaymentInformations = PaymentInformations = paymentInformation;
			PaymentPreferences = new PaymentDetailsViewModel(Guid.NewGuid().ToString(), paymentInformation);
			PaymentPreferences.SelectedCreditCardId = (Guid)account.DefaultCreditCard;
			PaymentPreferences.LoadCreditCards();
		}

		public PaymentInformation PaymentInformations { get; set ; }

		public PaymentInformation DefaultPaymentInformations { get; set ; }

		public PaymentDetailsViewModel PaymentPreferences
		{
			get;
			private set;
		}

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

