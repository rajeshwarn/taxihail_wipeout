using System;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Configuration.Impl;
using System.Windows.Input;
using ServiceStack.ServiceClient.Web;
using apcurium.MK.Booking.Mobile.Data;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Configuration;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.ViewModels.Payment
{
    public class CreditCardMultipleViewModel : PageViewModel
    {
        private readonly ILocationService _locationService;
        private readonly IPaymentService _paymentService;
        private readonly IAccountService _accountService;
        private readonly IAppSettings _appSettings;

		private ClientPaymentSettings _paymentSettings;

        private const int TipMaxPercent = 100;

        public CreditCardMultipleViewModel(
            ILocationService locationService,
            IPaymentService paymentService, 
			IAccountService accountService,
			IAppSettings appSettings)
        {
			_appSettings = appSettings;
            _locationService = locationService;
            _paymentService = paymentService;
            _accountService = accountService;
        }

		public override async void OnViewStarted(bool firstTime)
        {
            base.OnViewStarted(firstTime);
            // we stop the service when the viewmodel starts because it stops after the homeviewmodel starts when we press back
            // this ensures that we don't stop the service just after having started it in homeviewmodel
            _locationService.Stop();

			if (firstTime)
			{
				using (this.Services().Message.ShowProgress())
				{
					await GetCreditCArds();
				}	
			}
			else
			{
				await GetCreditCArds();
			}

        }

        public async override void Start()
        {
            base.Start();

            _paymentSettings = await _paymentService.GetPaymentSettings();
        }

        private async Task GetCreditCArds()
        {
            try
            {
                var creditCardsDetails = await _accountService.GetCreditCards();
				var defaultCreditCard = creditCardsDetails.First(cc => cc.CreditCardId == _accountService.CurrentAccount.DefaultCreditCard.CreditCardId);
				var orderedCreditCards = creditCardsDetails.ToList();
				orderedCreditCards.Remove(defaultCreditCard);
				orderedCreditCards.Insert(0, defaultCreditCard);
				CreditCards = orderedCreditCards.Select( cc => 
					{
						var cardNumber = string.Format("{0} **** {1} ", cc.Label, cc.Last4Digits);

						if(cc.CreditCardId == _accountService.CurrentAccount.DefaultCreditCard.CreditCardId)
						{
							cardNumber += "(DEFAULT)";
						}

						return new CreditCardInfos()
						{
							CardNumber = cardNumber,
							CreditCardId = cc.CreditCardId,
							CreditCardCompany = cc.CreditCardCompany
						};
					}).ToList();
			}
			catch
			{
				// Do nothing
			}
		}

		private List<CreditCardInfos> _creditCards;
		public List<CreditCardInfos> CreditCards
		{
			get
			{
				return _creditCards;
			}
			set
            {
                _creditCards = value;
                RaisePropertyChanged();
                RaisePropertyChanged(() => CanAddCard);
            }
        }

		public bool CanAddCard
		{
			get
			{
				return CreditCards.Count < _appSettings.Data.MaxNumberOfCardsOnFile;
			}
		}

        public bool ShouldDisplayTip
        {
            get
            {
                return _paymentSettings.IsPayInTaxiEnabled || _paymentSettings.PayPalClientSettings.IsEnabled;
            }
        }

        private PaymentDetailsViewModel _paymentPreferences;
        public PaymentDetailsViewModel PaymentPreferences
        {
            get
            {
                if (_paymentPreferences == null)
                {
                    _paymentPreferences = Container.Resolve<PaymentDetailsViewModel>();
                    _paymentPreferences.Start();
                    _paymentPreferences.ActionOnTipSelected = SaveTip;
                }
                return _paymentPreferences;
            }
        }

		public ICommand NavigateToDetails
		{
			get
			{
				return this.GetCommand<CreditCardInfos>( cci =>
					{
						ShowViewModel<CreditCardAddViewModel>(new {creditCardId = cci.CreditCardId, isFromMultiple = true});
					});
			}
		}

        public ICommand NavigateToAddCard
        {
            get
            {
                return this.GetCommand(() =>
                    {
						ShowViewModel<CreditCardAddViewModel>(new {isAddingNew = true, isFromMultiple = true});
                    });
            }
        }

        private ICommand SaveTip 
        { 
            get
            {
                return this.GetCommand<int>(async tip =>
                    {
                        if (PaymentPreferences.Tip > TipMaxPercent)
                        {
                            await this.Services().Message.ShowMessage(null, this.Services().Localize["TipPercent_Error"]);
                            return;
                        }

                        try
                        {
                            await _accountService.UpdateSettings(_accountService.CurrentAccount.Settings, _accountService.CurrentAccount.Email, PaymentPreferences.Tip);
                        }
                        catch (WebServiceException)
                        {
                            this.Services()
                                .Message.ShowMessage(this.Services().Localize["UpdateBookingSettingsInvalidDataTitle"],
                                    this.Services().Localize["UpdateBookingSettingsGenericError"]);
                        }
                    });
            } 
        } 
    }
}
