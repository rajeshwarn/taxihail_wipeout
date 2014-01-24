using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Entity;
using ServiceStack.Text;

namespace apcurium.MK.Booking.Mobile.ViewModels.Payment
{
    public class PaymentViewModel : BaseSubViewModel<object>
    {
        private readonly IPayPalExpressCheckoutService _palExpressCheckoutService;

		public PaymentViewModel(IPayPalExpressCheckoutService palExpressCheckoutService)
		{
			_palExpressCheckoutService = palExpressCheckoutService;
		}

		public void Init(string order, string orderStatus, IPayPalExpressCheckoutService palExpressCheckoutService)
        {
            this.Services().Config.GetPaymentSettings();

            Order = JsonSerializer.DeserializeFromString<Order>(order); 
            OrderStatus = orderStatus.FromJson<OrderStatusDetail>();

            var account = this.Services().Account.CurrentAccount;
            var paymentInformation = new PaymentInformation
            {
                CreditCardId = account.DefaultCreditCard,
                TipPercent = account.DefaultTipPercent,
            };
			PaymentPreferences = new PaymentDetailsViewModel();
			PaymentPreferences.Init(paymentInformation);

            PaymentSelectorToggleIsVisible = IsPayPalEnabled && IsCreditCardEnabled;
            PayPalSelected = !IsCreditCardEnabled;
        }

        public bool IsPayPalEnabled
        { 
            get
            {
                var payPalSettings = this.Services().Config.GetPaymentSettings().PayPalClientSettings;
                return payPalSettings.IsEnabled;
            }
        }

        public bool IsCreditCardEnabled
        { 
            get
            {
                var setting = this.Services().Config.GetPaymentSettings();
                return setting.IsPayInTaxiEnabled;
            }
        }

        Order Order { get; set; }

        OrderStatusDetail OrderStatus { get; set; }

		private bool _payPalSelected { get; set; }
        public bool PayPalSelected 
		{ 
			get { return _payPalSelected; }
			set {
				_payPalSelected = value;
				RaisePropertyChanged(() => PayPalSelected);
			}
		}

        public bool PaymentSelectorToggleIsVisible { get; set; }

        public AsyncCommand UsePayPal
        {
            get
            {
                return GetCommand(() => InvokeOnMainThread(delegate
                {
                    PayPalSelected = true;
                }));
            }
        }

        public AsyncCommand UseCreditCard
        {
            get
            {
                return GetCommand(() => InvokeOnMainThread(delegate
                {
                    PayPalSelected = false;
                }));
            }
        }

        public string PlaceholderAmount
        {
            get{ return CultureProvider.FormatCurrency(0d); }
        }

        public string TextAmount { get; set; }

        public double Amount
        { 
            get
            { 
                return CultureProvider.ParseCurrency(TextAmount);
            }
        }

		public string TipAmount { get; set; }

		public string MeterAmount { get; set; }

		public PaymentDetailsViewModel PaymentPreferences { get; private set; }

        public AsyncCommand ConfirmOrderCommand
        {
            get
            {
                return GetCommand(() =>
                {                    
                    Action executePayment = () =>
                    {
                        if (PayPalSelected)
                        {
                            PayPalFlow();
                        }
                        else
                        {
                            CreditCardFlow();
                        }
                    };

                    if (Amount > 100)
                    {
                        string message = string.Format(this.Services().Localize["ConfirmationPaymentAmountOver100"], CultureProvider.FormatCurrency(Amount));
						this.Services().Message.ShowMessage(this.Services().Localize["ConfirmationPaymentAmountOver100Title"], message, this.Services().Localize["OkButtonText"], () => Task.Factory.SafeStartNew(executePayment), this.Services().Localize["Cancel"], () => {});
                    }
                    else
                    {
                        executePayment();
                    }
                }); 
            }
        }

        private void PayPalFlow()
        {
            if (CanProceedToPayment(false))
            {
                this.Services().Message.ShowProgress(true);

                _palExpressCheckoutService.SetExpressCheckoutForAmount(Order.Id, Convert.ToDecimal(Amount), Convert.ToDecimal(CultureProvider.ParseCurrency(MeterAmount)), Convert.ToDecimal(CultureProvider.ParseCurrency(TipAmount)))
					.ToObservable()
                    // Always Hide progress indicator
                    .Do(_ => this.Services().Message.ShowProgress(false), _ => this.Services().Message.ShowProgress(false))
					.Subscribe(checkoutUrl => {
                        var @params = new Dictionary<string, string> { { "url", checkoutUrl } };
						ShowSubViewModel<PayPalViewModel, bool>(@params, success =>
                        {
                            if (success)
                            {
                                ShowPayPalPaymentConfirmation();
                                this.Services().Payment.SetPaymentFromCache(Order.Id, Amount);
                            }
                            else
                            {
                                this.Services().Message.ShowMessage(this.Services().Localize["PayPalExpressCheckoutCancelTitle"], this.Services().Localize["PayPalExpressCheckoutCancelMessage"]);
                            }
                        });
                }, error => { });
            }
        }

        private async void CreditCardFlow()
        {
            if (CanProceedToPayment())
            {
                using (this.Services().Message.ShowProgress())
                {
                    var response = await this.Services().Payment.PreAuthorizeAndCommit(PaymentPreferences.SelectedCreditCard.Token, Amount, CultureProvider.ParseCurrency(MeterAmount), CultureProvider.ParseCurrency(TipAmount), Order.Id);
                    if (!response.IsSuccessfull)
                    {
                        this.Services().Message.ShowProgress(false);
                        this.Services().Message.ShowMessage(this.Services().Localize["PaymentErrorTitle"], this.Services().Localize["CmtTransactionErrorMessage"]);
                        return;
                    }

                    this.Services().Payment.SetPaymentFromCache(Order.Id, Amount);
                    ShowCreditCardPaymentConfirmation(response.AuthorizationCode);
                }
            }
        }

        private bool CanProceedToPayment(bool requireCreditCard = true)
        {
            if (requireCreditCard && PaymentPreferences.SelectedCreditCard == null)
            {
                this.Services().Message.ShowProgress(false);
                this.Services().Message.ShowMessage(this.Services().Localize["PaymentErrorTitle"], this.Services().Localize["NoCreditCardSelected"]);
                return false;
            }

            if (Amount <= 0)
            {
                this.Services().Message.ShowProgress(false);
                this.Services().Message.ShowMessage(this.Services().Localize["PaymentErrorTitle"], this.Services().Localize["NoAmountSelectedMessage"]);
                return false;
            }

			if (!Order.IbsOrderId.HasValue)
            {
                this.Services().Message.ShowProgress(false);
                this.Services().Message.ShowMessage(this.Services().Localize["PaymentErrorTitle"], this.Services().Localize["NoOrderId"]);
                return false;
            }			

            return true;
        }

        private void ShowPayPalPaymentConfirmation()
        {
            this.Services().Message.ShowMessage(this.Services().Localize["PayPalExpressCheckoutSuccessTitle"],
                              this.Services().Localize["PayPalExpressCheckoutSuccessMessage"],
                              null, () => ReturnResult(""),
                              this.Services().Localize["OkButtonText"], () => ReturnResult(""));
        }

        private void ShowCreditCardPaymentConfirmation(string transactionId)
        {
            this.Services().Message.ShowMessage(this.Services().Localize["CmtTransactionSuccessTitle"],
                              string.Format(this.Services().Localize["CmtTransactionSuccessMessage"], transactionId),
                              null, () => ReturnResult(""),
                              this.Services().Localize["OkButtonText"], () => ReturnResult(""));
        }
    }
}

