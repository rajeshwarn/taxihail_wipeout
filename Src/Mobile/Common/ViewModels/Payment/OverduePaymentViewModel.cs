﻿using System;
using System.Globalization;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.AppServices;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels.Payment
{
	public class OverduePaymentViewModel : PageViewModel
	{
		private readonly IPaymentService _paymentService;
	    private readonly IAccountService _accountService;
	    private readonly IDeviceCollectorService _deviceCollectorService;
	    private readonly IAppSettings _appSettings;
        
        public OverduePaymentViewModel(IPaymentService paymentService, 
			IAccountService accountService,
            IDeviceCollectorService deviceCollectorService,
            IAppSettings appSettings)
		{
			_paymentService = paymentService;
		    _accountService = accountService;
            _deviceCollectorService = deviceCollectorService;
            _appSettings = appSettings;
        }

        private string _kountSessionId;

        public void Init(string overduePayment)
	    {
			OverduePayment = overduePayment.FromJson<OverduePayment>();

            _kountSessionId = _deviceCollectorService.GetSessionId();
        }

		private OverduePayment _overduePayment;
		public OverduePayment OverduePayment
		{
			get
			{
				return _overduePayment;
			}
			set
			{
				_overduePayment = value;

				RaisePropertyChanged();
				RaisePropertyChanged(() => AmountDue);
			}
		}

	    public override void OnViewStarted(bool firstTime)
	    {
	        base.OnViewStarted(firstTime);

	        if (!firstTime)
	        {
	            RaisePropertyChanged(() => HasCreditCard);
                RaisePropertyChanged(() => Last4Digits);
                RaisePropertyChanged(() => Company);
	        }
	    }

	    public decimal AmountDue
		{
			get
			{
				return _overduePayment != null
					? _overduePayment.OverdueAmount
					: 0;
			}
		}

	    public bool HasCreditCard
	    {
	        get { return _accountService.CurrentAccount.DefaultCreditCard != null; }
	    }

	    public string Last4Digits
	    {
	        get { return _accountService.CurrentAccount.DefaultCreditCard.SelectOrDefault(defaultCreditCard => defaultCreditCard.Last4Digits); }
	    }

	    public string Company
	    {
	        get { return _accountService.CurrentAccount.DefaultCreditCard.SelectOrDefault(defaultCreditCard => defaultCreditCard.CreditCardCompany); }
	    }

	    public bool CanShowOrderNumber
		{
			get
			{
				return Settings.ShowOrderNumber;
			}
		}

		public ICommand SettleOverduePayment
		{
			get
			{
				return this.GetCommand(async () => 
				{
                    var localize = this.Services().Localize;

				    using (this.Services().Message.ShowProgress())
				    {
                        try
                        {
                            var overduePaymentResult = await _paymentService.SettleOverduePayment(_kountSessionId);

                            if (overduePaymentResult.IsSuccessful)
                            {
                                // Fire and forget to update creditcard cache, we do not need to wait for this.
                                Task.Run(() => _accountService.GetDefaultCreditCard()).FireAndForget();

                                var message = string.Format(localize["Overdue_Succeed_Message"],
                                    string.Format(new CultureInfo(Settings.PriceFormat), localize["CurrencyPriceFormat"], _overduePayment.OverdueAmount));

                                await this.Services().Message.ShowMessage(localize["Overdue_Succeed_Title"], message);

                                Close(this);
                            }
                            else
                            {
                                await this.Services().Message.ShowMessage(localize["Overdue_Failed_Title"], localize["Overdue_Failed_Message"]);
                            }
                        }
                        catch(Exception ex)
                        {
                            Logger.LogError(ex);
                            this.Services().Message.ShowMessage(localize["Overdue_Failed_Title"], localize["Overdue_Failed_Message"]);
                        }
				    }
				});
			}
		}

		public ICommand UpdateCreditCard
		{
			get
			{
				return this.GetCommand(() => 
				{
					if(_appSettings.Data.MaxNumberOfCardsOnFile > 1)
					{
						ShowViewModel<CreditCardMultipleViewModel>(new 
						{
                            hasPaymentToSettle = true 
						});
					}
					else
					{
						ShowViewModel<CreditCardAddViewModel>(new 
						{
                            hasPaymentToSettle = true 
						});
					}
				});
			}
		}
	}
}

