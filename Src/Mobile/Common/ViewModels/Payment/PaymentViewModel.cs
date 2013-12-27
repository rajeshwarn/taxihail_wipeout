using System;
using apcurium.MK.Booking.Mobile.AppServices;
using ServiceStack.Text;
using Cirrious.MvvmCross.ExtensionMethods;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using Cirrious.MvvmCross.Interfaces.Commands;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Entity;
using System.Collections.Generic;
using System.Reactive.Threading.Tasks;
using System.Reactive.Linq;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class PaymentViewModel : BaseSubViewModel<object>, IMvxServiceConsumer<IPayPalExpressCheckoutService>
    {
        public PaymentViewModel(string order, string orderStatus, string messageId) : base(messageId)
        {
            ConfigurationManager.GetPaymentSettings(false);

            Order = JsonSerializer.DeserializeFromString<Order>(order); 
            OrderStatus = orderStatus.FromJson<OrderStatusDetail>();  

            var account = AccountService.CurrentAccount;
            var paymentInformation = new PaymentInformation
            {
                CreditCardId = account.DefaultCreditCard,
                TipPercent = account.DefaultTipPercent,
            };
            PaymentPreferences = new PaymentDetailsViewModel(Guid.NewGuid().ToString(), paymentInformation);

            PaymentSelectorToggleIsVisible = IsPayPalEnabled && IsCreditCardEnabled;
            PayPalSelected = !IsCreditCardEnabled;
        }

        public bool IsPayPalEnabled
        { 
            get
            {
                var payPalSettings = ConfigurationManager.GetPaymentSettings().PayPalClientSettings;
                return payPalSettings.IsEnabled;
            }
        }

        public bool IsCreditCardEnabled
        { 
            get
            {
                var setting = ConfigurationManager.GetPaymentSettings();
                return setting.IsPayInTaxiEnabled;
            }
        }

        Order Order { get; set; }

        OrderStatusDetail OrderStatus { get; set; }

        public bool PayPalSelected { get; set; }

        public bool PaymentSelectorToggleIsVisible { get; set; }

        public IMvxCommand UsePayPal
        {
            get
            {
                return GetCommand(() => InvokeOnMainThread(delegate
                {
                    PayPalSelected = true;
                    FirePropertyChanged(() => PayPalSelected);
                }));
            }
        }

        public IMvxCommand UseCreditCard
        {
            get
            {
                return GetCommand(() => InvokeOnMainThread(delegate
                {
                    PayPalSelected = false;
                    FirePropertyChanged(() => PayPalSelected);
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

        public string TipAmount
        {
            get;
            set;
        }

        public string MeterAmount
        {
            get;
            set;
        }

        public PaymentDetailsViewModel PaymentPreferences
        {
            get;
            private set;
        }

        public IMvxCommand ConfirmOrderCommand
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

                        string message = string.Format(Resources.GetString("ConfirmationPaymentAmountOver100"),CultureProvider.FormatCurrency(Amount) );
                            
                        MessageService.ShowMessage(Resources.GetString("ConfirmationPaymentAmountOver100Title"), message, Resources.GetString("OkButtonText"), () => Task.Factory.SafeStartNew(executePayment), Resources.GetString("CancelBoutton"), Nothing);

                    }
                    else
                    {
                        executePayment();
                    }
                }); 
            }
        }

        private void Nothing()
        {
        }

        private void PayPalFlow()
        {
            if (CanProceedToPayment(requireCreditCard: false))
            {
                MessageService.ShowProgress(true);
                var paypal = this.GetService<IPayPalExpressCheckoutService>();
                paypal.SetExpressCheckoutForAmount(Order.Id, Convert.ToDecimal(Amount), Convert.ToDecimal(CultureProvider.ParseCurrency(MeterAmount)), Convert.ToDecimal(CultureProvider.ParseCurrency(TipAmount)))
					.ToObservable()
                    // Always Hide progress indicator
					.Do(_ => MessageService.ShowProgress(false), _ => MessageService.ShowProgress(false))
					.Subscribe(checkoutUrl => {
                        var @params = new Dictionary<string, string> { { "url", checkoutUrl } };
                        RequestSubNavigate<PayPalViewModel, bool>(@params, success =>
                        {
                            if (success)
                            {
                                ShowPayPalPaymentConfirmation();
                                PaymentService.SetPaymentFromCache(Order.Id, Amount);
                            }
                            else
                            {
                                MessageService.ShowMessage(Resources.GetString("PayPalExpressCheckoutCancelTitle"), Resources.GetString("PayPalExpressCheckoutCancelMessage"));
                            }
                        });
                }, error => { });
            }
        }

        private void CreditCardFlow()
        {
            if (CanProceedToPayment())
            {
                using (MessageService.ShowProgress ())
                {
                    var response = PaymentService.PreAuthorizeAndCommit(PaymentPreferences.SelectedCreditCard.Token, Amount, CultureProvider.ParseCurrency(MeterAmount), CultureProvider.ParseCurrency(TipAmount), Order.Id);
                    if (!response.IsSuccessfull)
                    {
                        MessageService.ShowProgress(false);
                        MessageService.ShowMessage(Resources.GetString("PaymentErrorTitle"), Str.CmtTransactionErrorMessage);
                        return;
                    }

                    PaymentService.SetPaymentFromCache(Order.Id, Amount);
                    ShowCreditCardPaymentConfirmation(response.AuthorizationCode);
                }
            }
        }

        private bool CanProceedToPayment(bool requireCreditCard = true)
        {
            if (requireCreditCard && PaymentPreferences.SelectedCreditCard == null)
            {
                MessageService.ShowProgress(false);
                MessageService.ShowMessage(Resources.GetString("PaymentErrorTitle"), Str.NoCreditCardSelectedMessage);
                return false;
            }

            if (Amount <= 0)
            {
                MessageService.ShowProgress(false);
                MessageService.ShowMessage(Resources.GetString("PaymentErrorTitle"), Str.NoAmountSelectedMessage);
                return false;
            }

			if (!Order.IbsOrderId.HasValue)
            {
                MessageService.ShowProgress(false);
                MessageService.ShowMessage(Resources.GetString("PaymentErrorTitle"), Str.NoOrderId);
                return false;
            }			

            return true;
        }

        private void ShowPayPalPaymentConfirmation()
        {
            MessageService.ShowMessage(Resources.GetString("PayPalExpressCheckoutSuccessTitle"),
                              Resources.GetString("PayPalExpressCheckoutSuccessMessage"),
                              null, () => ReturnResult(""),
                              Str.OkButtonText, () => ReturnResult(""));
        }

        private void ShowCreditCardPaymentConfirmation(string transactionId)
        {
            MessageService.ShowMessage(Str.CmtTransactionSuccessTitle,
                              string.Format(Str.CmtTransactionSuccessMessage, transactionId),
                              null, () => ReturnResult(""),
                              Str.OkButtonText, () => ReturnResult(""));
        }
    }
}

