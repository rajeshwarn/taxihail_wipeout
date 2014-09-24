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
using System.Globalization;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Mobile.ViewModels.Payment
{
	public class PaymentViewModel : PageViewModel
	{
		private readonly IPayPalExpressCheckoutService _paypalExpressCheckoutService;
		private readonly IAccountService _accountService;
		private readonly IPaymentService _paymentService;
		private readonly ILocalization _localize;


		public PaymentViewModel(IPayPalExpressCheckoutService paypalExpressCheckoutService,
			IAccountService accountService,
			IPaymentService paymentService,
			ILocalization localize)
		{
			_paypalExpressCheckoutService = paypalExpressCheckoutService;
			_accountService = accountService;
			_paymentService = paymentService;
			_localize = localize;
		}

		public void Init(string order, string orderStatus)
		{
			_paymentService.GetPaymentSettings();

            Order = JsonSerializer.DeserializeFromString<Order>(order); 
            OrderStatus = orderStatus.FromJson<OrderStatusDetail>();

			PaymentPreferences = Container.Resolve<PaymentDetailsViewModel>();
			PaymentPreferences.Start();
			TipAmount = RoundToTwoDecimals((MeterAmount.ToDouble() * ((double)PaymentPreferences.Tip / 100)));

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
				MeterAmount = RoundToTwoDecimals (value);
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
				TipAmount = RoundToTwoDecimals(order.Tip.Value);
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
			set 
			{
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
			get{ return RoundToTwoDecimals(0d); }
        }
		       
		private string _tipAmount;
		public string TipAmount 
		{ 
			get
			{				
				return GetTipAmount(_tipAmount);
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
			get { return _meterAmount; }
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
				// the ONLY currency formatted amount
				return CultureProvider.FormatCurrency(Amount);
			}	
		}

		public double Amount
		{ 
			get
			{ 
				return MeterAmount.ToDouble() + GetTipAmount(TipAmount).ToDouble();
			}
		}

		public string TipAmountString  { get; set;}

		public string GetTipAmount(string value)
		{
			return PaymentPreferences.TipListDisabled 
				? value 
				: RoundToTwoDecimals (MeterAmount.ToDouble() * ((double)PaymentPreferences.Tip / 100));
		}
			
		private string GetCurrency(string amount)
		{
			return RoundToTwoDecimals (amount.ToDouble());
		}

		private string RoundToTwoDecimals(double doubleNumber)
		{
			return doubleNumber.ToString("F", CultureInfo.CurrentUICulture);
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
					if (IsResettingTip && PaymentPreferences.Tip == 0 && TipAmount.ToDouble() == 0d)
                    {
                        IsResettingTip = false;
                        return;
                    }
                    IsResettingTip = false;
                    PaymentPreferences.TipListDisabled = false;                                            
					TipAmount = RoundToTwoDecimals((MeterAmount.ToDouble() * ((double)PaymentPreferences.Tip / 100)));
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
						TipAmountString = RoundToTwoDecimals(TipAmount.ToDouble());
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
						var message = string.Format(this.Services().Localize["ConfirmationPaymentAmountOver100"], TotalAmount);
						this.Services().Message.ShowMessage(
								this.Services().Localize["ConfirmationPaymentAmountOver100Title"], message, 
								this.Services().Localize["OkButtonText"], () => InvokeOnMainThread(executePayment), 
								this.Services().Localize["Cancel"], () => {});
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
				_paypalExpressCheckoutService.SetExpressCheckoutForAmount(
						Order.Id,
						Convert.ToDecimal(Amount), 
						Convert.ToDecimal(MeterAmount.ToDouble()), 
						Convert.ToDecimal(RoundToTwoDecimals(TipAmount.ToDouble())),
						Order.IBSOrderId,
						TotalAmount,
						_localize.CurrentLanguage)
					        .ToObservable()
					        .Subscribe(checkoutUrl => {
                                var @params = new Dictionary<string, string> { { "url", checkoutUrl } };
						        this.Services().Message.ShowProgress(false);
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
					var meterAmount = MeterAmount.ToDouble();
					var tipAmount = RoundToTwoDecimals(TipAmount.ToDouble()).ToDouble();
					var response = await _paymentService.PreAuthorizeAndCommit(PaymentPreferences.SelectedCreditCard.Token, Amount, meterAmount, tipAmount, Order.Id);
                    if (!response.IsSuccessfull)
                    {
						this.Services().Message.ShowProgress(false);
						this.Services().Message.ShowMessage(
							this.Services().Localize["PaymentErrorTitle"], 
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

        private async void ShowPayPalPaymentConfirmation()
        {
            await this.Services().Message.ShowMessage(this.Services().Localize["PayPalExpressCheckoutSuccessTitle"],
                              this.Services().Localize["PayPalExpressCheckoutSuccessMessage"]);
			Close(this);
        }

        private async void ShowCreditCardPaymentConfirmation(string transactionId)
        {
            await this.Services().Message.ShowMessage(this.Services().Localize["CmtTransactionSuccessTitle"],
                              string.Format(this.Services().Localize["CmtTransactionSuccessMessage"], transactionId));
			Close(this);
        }
    }
}
