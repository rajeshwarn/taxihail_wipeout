using System;
using System.Globalization;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.AppServices;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking.Mobile.ViewModels.Payment
{
	public class OverduePaymentViewModel : PageViewModel
	{
		private readonly IPaymentService _paymentService;
	    private readonly IAccountService _accountService;
		private readonly IAppSettings _appSettings;

		public OverduePaymentViewModel(IPaymentService accountService, 
			IAccountService accountService1,
			IAppSettings appSettings)
		{
			this._appSettings = appSettings;
		    _paymentService = accountService;
		    _accountService = accountService1;
		}

	    public void Init(string overduePayment)
	    {
			OverduePayment = overduePayment.FromJson<OverduePayment>();
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

		public decimal AmountDue
		{
			get
			{
				return _overduePayment != null
					? _overduePayment.OverdueAmount
					: 0;
			}
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
                            var overduePaymentResult = await _paymentService.SettleOverduePayment();

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
					var serializedOverduePayment = _overduePayment.ToJson();
					if(_appSettings.Data.MaxNumberOfCardsOnFile > 1)
					{
						ShowViewModel<CreditCardMultipleViewModel>(new 
							{ 
								paymentToSettle = serializedOverduePayment 
							});
					}
					else
					{
						ShowViewModel<CreditCardAddViewModel>(new 
							{ 
								paymentToSettle = serializedOverduePayment 
							});
					}
				});
			}
		}
	}
}

