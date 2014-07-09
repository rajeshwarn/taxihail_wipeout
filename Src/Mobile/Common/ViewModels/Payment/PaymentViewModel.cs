using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Windows.Input;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Entity;
using ServiceStack.Text;

namespace apcurium.MK.Booking.Mobile.ViewModels.Payment
{
	public class PaymentViewModel : PageViewModel
	{
        private readonly IPayPalExpressCheckoutService _palExpressCheckoutService;
		private readonly IAccountService _accountService;
		private readonly IPaymentService _paymentService;

		public PaymentViewModel(IPayPalExpressCheckoutService palExpressCheckoutService,
			IAccountService accountService,
			IPaymentService paymentService)
		{
			_palExpressCheckoutService = palExpressCheckoutService;
			_accountService = accountService;
			_paymentService = paymentService;
		}

		public void Init(string order, string orderStatus)
		{
			_paymentService.GetPaymentSettings();

            Order = JsonSerializer.DeserializeFromString<Order>(order); 
            OrderStatus = orderStatus.FromJson<OrderStatusDetail>();

			PaymentPreferences = Container.Resolve<PaymentDetailsViewModel>();
			PaymentPreferences.Start();
			TipAmount = (CultureProvider.ParseCurrency(MeterAmount) * ((double)PaymentPreferences.Tip / 100)).ToString();

            PaymentSelectorToggleIsVisible = IsPayPalEnabled && IsCreditCardEnabled;
            PayPalSelected = !IsCreditCardEnabled;

            PaymentPreferences.TipListDisabled = false;

			InitAmounts(Order);
        }

		public override async void OnViewLoaded ()
		{
			base.OnViewLoaded ();

			var orderFromServer =  await _accountService.GetHistoryOrderAsync(Order.Id);
			InitAmounts(orderFromServer);
		}

		void InitAmounts(Order order)
		{
			if (order == null)
				return;

			if (order.Fare.HasValue && order.Fare.Value != 0)
			{
				var value = order.Fare.Value + (order.Toll.HasValue ? order.Toll.Value : 0);
				Logger.LogMessage ("Meter Amount not formatted : {0} for Order {1}", value, Order.IBSOrderId); 
				MeterAmount = CultureProvider.FormatCurrency (value);
				Logger.LogMessage ("Meter Amount : {0} for Order {1}", MeterAmount, Order.IBSOrderId); 
				MeterAmountPopulatedByIBS = true;
			}
			else
			{
				MeterAmountPopulatedByIBS = false;
			}

			if (order.Tip.HasValue 
				&& order.Tip.Value != 0)
			{
				PaymentPreferences.TipListDisabled = true;
				TipAmount = CultureProvider.FormatCurrency(order.Tip.Value);
			}
		}

		private bool _meterAmountPopulatedByIBS;
		public bool MeterAmountPopulatedByIBS
		{
			get { return _meterAmountPopulatedByIBS; }
			set
			{
				_meterAmountPopulatedByIBS = value;
				RaisePropertyChanged ();
			}
		}

        public bool IsPayPalEnabled
        { 
            get
            {
				var payPalSettings = _paymentService.GetPaymentSettings().PayPalClientSettings;
                return payPalSettings.IsEnabled;
            }
        }

        public bool IsCreditCardEnabled
        { 
            get
            {
				var setting = _paymentService.GetPaymentSettings();
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
				var roundedAmount =  Math.Round((CultureProvider.ParseCurrency (MeterAmount) * ((double)PaymentPreferences.Tip / 100)), 2);
                return (PaymentPreferences.TipListDisabled ? _tipAmount : roundedAmount.ToString());
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
			var roundedTip = Math.Round(CultureProvider.ParseCurrency (MeterAmount) * ((double)PaymentPreferences.Tip / 100), 2);
			var _tip = (PaymentPreferences.TipListDisabled ? value : roundedTip.ToString());
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
					if (MeterAmount != null && MeterAmount != GetCurrency(MeterAmount))
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
						var message = string.Format(this.Services().Localize["ConfirmationPaymentAmountOver100"], CultureProvider.FormatCurrency(Amount));
						this.Services().Message.ShowMessage(
								this.Services().Localize["ConfirmationPaymentAmountOver100Title"], 
								message, 
								this.Services().Localize["OkButtonText"], 
								() => InvokeOnMainThread(executePayment), 
								this.Services().Localize["Cancel"], 
								() => {});
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
				_palExpressCheckoutService.SetExpressCheckoutForAmount(Order.Id, Convert.ToDecimal(Amount), Convert.ToDecimal(CultureProvider.ParseCurrency(MeterAmount)), Convert.ToDecimal(Math.Round(CultureProvider.ParseCurrency (TipAmount), 2)))
					.ToObservable()
					.Subscribe(checkoutUrl => {
                        var @params = new Dictionary<string, string> { { "url", checkoutUrl } };
						ShowSubViewModel<PayPalViewModel, bool>(@params, success =>
                        {
                            if (success)
                            {
                                ShowPayPalPaymentConfirmation();
								_paymentService.SetPaymentFromCache(Order.Id, Amount);
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
			using (this.Services().Message.ShowProgress())
			{
	            if (CanProceedToPayment())
	            {
					var meterAmount = CultureProvider.ParseCurrency (MeterAmount);
					var tipAmount = Math.Round(CultureProvider.ParseCurrency (TipAmount), 2);
					var response = await _paymentService.PreAuthorizeAndCommit(PaymentPreferences.SelectedCreditCard.Token, Amount, meterAmount, tipAmount, Order.Id);
                    if (!response.IsSuccessfull)
                    {
						this.Services().Message.ShowProgress(false);
						this.Services().Message.ShowMessage(this.Services().Localize["PaymentErrorTitle"], 
							string.Format(this.Services().Localize["PaymentErrorMessage"], response.Message));
						return;
                    }

					_paymentService.SetPaymentFromCache(Order.Id, Amount);
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