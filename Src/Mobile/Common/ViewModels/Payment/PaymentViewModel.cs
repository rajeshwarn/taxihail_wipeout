using System;
using apcurium.MK.Booking.Mobile.AppServices;
using ServiceStack.Text;
using Cirrious.MvvmCross.ExtensionMethods;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using Cirrious.MvvmCross.Interfaces.Commands;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Api.Contract.Resources;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.AppServices.Impl;
using System.Collections.Generic;
using System.Reactive.Threading.Tasks;
using System.Reactive.Linq;
using System.Threading;


namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class PaymentViewModel : BaseSubViewModel<object>, IMvxServiceConsumer<IPayPalExpressCheckoutService>
    {

        public PaymentViewModel (string order, string orderStatus, string messageId) : base(messageId)
        {
			ConfigurationManager.GetPaymentSettings (true);
			PaymentClient = null;//ensure that the payment settings are reloaded at least once

			Order = JsonSerializer.DeserializeFromString<Order>(order); 
			OrderStatus = orderStatus.FromJson<OrderStatusDetail>();  

            var account = AccountService.CurrentAccount;
			var paymentInformation = new PaymentInformation 
			{
                CreditCardId = account.DefaultCreditCard,
                TipPercent = account.DefaultTipPercent,
            };
            PaymentPreferences = new PaymentDetailsViewModel(Guid.NewGuid().ToString(), paymentInformation);
			PayPalSelected = false;
        }

		public override void Start (bool firstStart)
		{
			base.Start (firstStart);
		}
		
		public bool IsPayPalEnabled{ get{
				var payPalSettings = ConfigurationManager.GetPaymentSettings ().PayPalClientSettings;
				return payPalSettings.IsEnabled;
			}}

		Order Order { get; set; }

		OrderStatusDetail OrderStatus { get; set; }

		public bool PayPalSelected { get; set; }

		public string PlaceholderAmount
		{
			get{ return CultureProvider.FormatCurrency (0d); }
		}

		public string TextAmount { get; set;}
		public double Amount
		{ 
			get 
			{ 
				return CultureProvider.ParseCurrency (TextAmount);
			}
		}

        public PaymentDetailsViewModel PaymentPreferences {
            get;
            private set;
        }

		public bool ConfirmPaymentForDriver(){
			
			try
			{
				var formattedAmount = CultureProvider.FormatCurrency(Amount); 
				return VehicleClient.SendMessageToDriver(OrderStatus.VehicleNumber, Str.GetPaymentConfirmationMessageToDriver(formattedAmount));
			}
			catch(Exception){
			}
			
			return false;
		}

		public void ShowConfirmation(string transactionId)
		{
            MessageService.ShowMessage(Str.CmtTransactionSuccessTitle, string.Format(Str.CmtTransactionSuccessMessage, transactionId),
			                            Str.CmtTransactionResendConfirmationButtonText, ()=>
			{				
				ConfirmPaymentForDriver();
                ShowConfirmation(transactionId);
			},
			Str.OkButtonText, ()=> ReturnResult(""));
		}

		public IMvxCommand UsePayPal
		{
			get
			{
				return GetCommand(() => this.InvokeOnMainThread(delegate
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
				return GetCommand(() => this.InvokeOnMainThread(delegate
				                                                {
					PayPalSelected = false;
					FirePropertyChanged(() => PayPalSelected);
				}));
			}
		}

        public IMvxCommand ConfirmOrderCommand
        {
            get
            {
                return GetCommand(() => 
                {                    
					if(PayPalSelected){
						PayPalFlow();
					}
					else {
						CreditCardFlow();
					}
                }); 
            }
        }

		private void PayPalFlow()
		{
			if(CanProceedToPayment(requireCreditCard: false))
			{
				MessageService.ShowProgress(true);
				var paypal = this.GetService<IPayPalExpressCheckoutService>();
				paypal.SetExpressCheckoutForAmount(Order.Id, Convert.ToDecimal(Amount))
					.ToObservable()
						// Always Hide progress indicator
						.Do(_=> MessageService.ShowProgress(false), _=> MessageService.ShowProgress(false))
						.Subscribe(checkoutUrl => {

							var @params = new Dictionary<string, string>() {
								{"url", checkoutUrl},
							};
							this.RequestSubNavigate<PayPalViewModel, bool>(@params, success => {
								if(success)
								{
									MessageService.ShowMessage(Resources.GetString("PayPalExpressCheckoutSuccessTitle"), Resources.GetString("PayPalExpressCheckoutSuccessMessage"),
									                           Str.CmtTransactionResendConfirmationButtonText, ()=> ConfirmPaymentForDriver(), Str.OkButtonText, ()=> ReturnResult(""));
								} else {
									MessageService.ShowMessage(Resources.GetString("PayPalExpressCheckoutCancelTitle"), Resources.GetString("PayPalExpressCheckoutCancelMessage"));
								}
							});
						}, error => {

						});
			}
		}

		private void CreditCardFlow()
		{
			if(CanProceedToPayment())
			{
				using(MessageService.ShowProgress ())
				{
					var preAuthResponse = PaymentClient.PreAuthorize(PaymentPreferences.SelectedCreditCard.Token,  Amount, Order.Id);

					if (!preAuthResponse.IsSuccessfull)
					{
						MessageService.ShowProgress(false);
						MessageService.ShowMessage (Str.ErrorCreatingOrderTitle, Str.CmtTransactionErrorMessage);
						return;
					}
					// Give the backend some time to proccess the previous command
					Thread.Sleep(500);
					try {
						PaymentClient.CommitPreAuthorized(preAuthResponse.TransactionId);
					}
					catch(Exception e)
					{
						MessageService.ShowMessage (Str.ErrorCreatingOrderTitle, Str.TaxiServerDownMessage);
						return;
					}

					ShowConfirmation(preAuthResponse.TransactionId);					          
				}
			}
		}

		private bool CanProceedToPayment(bool requireCreditCard = true)
		{
			if(requireCreditCard && PaymentPreferences.SelectedCreditCard == null)
			{
				MessageService.ShowProgress(false);
				MessageService.ShowMessage (Str.ErrorCreatingOrderTitle, Str.NoCreditCardSelectedMessage);
				return false;
			}

			if(Amount <= 0)
			{
				MessageService.ShowProgress(false);
				MessageService.ShowMessage (Str.ErrorCreatingOrderTitle, Str.NoAmountSelectedMessage);
				return false;
			}

			if(!Order.IBSOrderId.HasValue)
			{
				MessageService.ShowProgress(false);
				MessageService.ShowMessage (Str.ErrorCreatingOrderTitle, Str.NoOrderId);
				return false;
			}			

			return true;
		}

    }
}

