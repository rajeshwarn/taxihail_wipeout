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
using System.Windows.Input;

namespace apcurium.MK.Booking.Mobile.ViewModels.Payment
{
    public class PaymentViewModel : BaseViewModel
    {
        private readonly IPayPalExpressCheckoutService _palExpressCheckoutService;
		private readonly IAccountService _accountService;

		public PaymentViewModel(IPayPalExpressCheckoutService palExpressCheckoutService,
			IAccountService accountService)
		{
			_accountService = accountService;
			_palExpressCheckoutService = palExpressCheckoutService;
		}

		public async void Init(string order, string orderStatus)
        {
			this.Services().Payment.GetPaymentSettings();

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
			TipAmount = (CultureProvider.ParseCurrency(MeterAmount) * ((double)PaymentPreferences.Tip / 100)).ToString();

            PaymentSelectorToggleIsVisible = IsPayPalEnabled && IsCreditCardEnabled;
            PayPalSelected = !IsCreditCardEnabled;

            PaymentPreferences.TipListDisabled = false;

			InitAmounts(Order);

			//refresh from the server
			var orderFromServer = await _accountService.GetHistoryOrderAsync(Order.Id);
			InitAmounts(orderFromServer);
        }

		void InitAmounts(Order order)
		{
			if (order == null)
				return;

			if (order.Fare.HasValue)
			{
				double value = order.Fare.Value + (order.Toll.HasValue ? order.Toll.Value : 0);
				MeterAmount = CultureProvider.FormatCurrency(value);
			}

			if (order.Tip.HasValue)
			{
				PaymentPreferences.TipListDisabled = true;
				TipAmount = CultureProvider.FormatCurrency(order.Tip.Value);
			}
		}

        public bool IsPayPalEnabled
        { 
            get
            {
				var payPalSettings = this.Services().Payment.GetPaymentSettings().PayPalClientSettings;
                return payPalSettings.IsEnabled;
            }
        }

        public bool IsCreditCardEnabled
        { 
            get
            {
				var setting = this.Services().Payment.GetPaymentSettings();
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

        public bool TipSelectorIsVisible { get; set; }

		public ICommand UsePayPal
        {
            get
            {

				return this.GetCommand(() => InvokeOnMainThread(delegate
                {
                    PayPalSelected = true;
                }));
            }
        }

		public ICommand UseCreditCard
        {
            get
            {
                return this.GetCommand(() => InvokeOnMainThread(delegate
                {
                    PayPalSelected = false;
                }));
            }
        }

        public string PlaceholderAmount
        {
            get{ return CultureProvider.FormatCurrency(0d); }
        }

        public double Amount
        { 
            get
            { 
				return CultureProvider.ParseCurrency(TotalAmount);
            }
        }

		private string _tipAmount;

		public string TipAmount 
		{ 
			get
			{				 
                return (PaymentPreferences.TipListDisabled ? _tipAmount : (CultureProvider.ParseCurrency(MeterAmount) * ((double)PaymentPreferences.Tip / 100)).ToString());
			}

			set
			{
				_tipAmount = GetTipAmount(value);
				TipAmountString = GetCurrency(_tipAmount);
                RaisePropertyChanged(() => TipAmount);
                RaisePropertyChanged(() => TotalAmount);			
			}		
		}

		private string _meterAmount;

		public string MeterAmount 
		{ 
			get
			{
				return _meterAmount;
			}

			set
			{
				_meterAmount = value;
				TipAmountString = GetCurrency(TipAmount);
				RaisePropertyChanged(() => MeterAmount);
				RaisePropertyChanged(() => TotalAmount);
				RaisePropertyChanged(() => TipAmountString);
			}		
		}

		public string TotalAmount 
		{ 
			get
			{
				return GetCurrency((CultureProvider.ParseCurrency(MeterAmount) + CultureProvider.ParseCurrency(GetTipAmount(TipAmount))).ToString());
			}	
		}

		public string TipAmountString  { get; set;}

		public string GetTipAmount(string value)
		{
            var _tip = (PaymentPreferences.TipListDisabled ? value : (CultureProvider.ParseCurrency(MeterAmount) * ((double)PaymentPreferences.Tip / 100)).ToString());
			return _tip;
		}

		public string GetCurrency(string amount)
		{
			return CultureProvider.FormatCurrency((CultureProvider.ParseCurrency(amount)));
		}

        public bool IsResettingTip = false; // For Droid's tip picker

		public ICommand ToggleToTipCustom
		{
			get
			{
				return this.GetCommand(() =>
				{ 
                    if (!PaymentPreferences.TipListDisabled)
                    {
                        IsResettingTip = true;
                        PaymentPreferences.Tip = 0;
                        PaymentPreferences.TipListDisabled = true;
					    RaisePropertyChanged(() => TotalAmount);
                        RaisePropertyChanged(() => PaymentPreferences);
                        ClearTipCommand.Execute();
                    }
				});
			}
		}

		public ICommand ToggleToTipSelector
        {
            get
            {
                return this.GetCommand(() =>
                { 
                    if (IsResettingTip && PaymentPreferences.Tip == 0 && CultureProvider.ParseCurrency(TipAmount) == 0d)
                    {
                        IsResettingTip = false;
                        return;
                    }
                    IsResettingTip = false;
                    PaymentPreferences.TipListDisabled = false;                                            
                    TipAmount = (CultureProvider.ParseCurrency(MeterAmount) * ((double)PaymentPreferences.Tip / 100)).ToString();
                    RaisePropertyChanged(() => TotalAmount);
                    RaisePropertyChanged(() => TipAmountString);
                    ShowCurrencyCommand.Execute(); 
                    RaisePropertyChanged(() => PaymentPreferences);
                });
            }
        }

		public ICommand ClearTipCommand
		{
			get
			{
				return this.GetCommand(() =>
					{ 					
                        TipAmount = "";	
                        TipAmountString = "";						
						RaisePropertyChanged(() => TipAmountString);
					});
			}
		}

		public ICommand ClearMeterCommand
		{
			get
			{
				return this.GetCommand(() =>
				{ 					
					MeterAmount = "";
					RaisePropertyChanged(() => MeterAmount);
				});
			}
		}

		public ICommand ShowCurrencyCommand
        {
            get
            {
                return this.GetCommand(() =>
                { 										
                    if (MeterAmount.ToString() != GetCurrency(MeterAmount).ToString())
                    {
                        MeterAmount = GetCurrency(MeterAmount);
                    }
                    if (TipAmountString != GetCurrency(TipAmountString))
                    {
                        TipAmountString = GetCurrency(CultureProvider.ParseCurrency(TipAmount).ToString());
                    }
                    RaisePropertyChanged(() => TipAmountString);							
                });
            }
        }

		public PaymentDetailsViewModel PaymentPreferences { get; private set; }

		public ICommand ConfirmOrderCommand
        {
            get
            {
                return this.GetCommand(() =>
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

			if (!Order.IBSOrderId.HasValue)
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
							  null, () => {Close(this);},
							  this.Services().Localize["OkButtonText"], () => {Close(this);});
        }

        private void ShowCreditCardPaymentConfirmation(string transactionId)
        {
            this.Services().Message.ShowMessage(this.Services().Localize["CmtTransactionSuccessTitle"],
                              string.Format(this.Services().Localize["CmtTransactionSuccessMessage"], transactionId),
								null, () => {Close(this);},
								this.Services().Localize["OkButtonText"], () => {Close(this);});
        }
    }
}

