using System;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Extensions;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.Data;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Common.Configuration;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.ViewModels.Payment
{
	public class CreditCardMultipleViewModel : CreditCardBaseViewModel
    {
        private readonly IAccountService _accountService;
        private readonly IAppSettings _appSettings;

		private bool _hasPaymentToSettle;

        public CreditCardMultipleViewModel(
            ILocationService locationService,
            IPaymentService paymentService, 
			IAccountService accountService,
			IAppSettings appSettings)
			:base(locationService, paymentService, accountService)
        {
			_appSettings = appSettings;
            _accountService = accountService;
        }

	    public void Init(bool hasPaymentToSettle)
	    {
	        _hasPaymentToSettle = hasPaymentToSettle;
	    }

		public override async void BaseOnViewStarted(bool firstTime)
        {
			if (firstTime)
			{
				using (this.Services().Message.ShowProgress())
				{
					await GetCreditCards();
				}	
			}
			else
			{
				await GetCreditCards();

			}
        }

		public override async void BaseStart()
		{

			if (_hasPaymentToSettle)
			{
				return;
			}

			await GoToOverduePayment();
		}

        private async Task GetCreditCards()
        {
            try
            {
                var creditCardsDetails = await _accountService.GetCreditCards();

				CreditCards = creditCardsDetails
					.Select( cc => 
						{
							var creditCardInfos =  new CreditCardInfos
							{
								CreditCardId = cc.CreditCardId,
								CreditCardCompany = cc.CreditCardCompany
							};
							var cardNumber = string.Format("{0} **** {1} ", cc.Label, cc.Last4Digits);

							if(cc.CreditCardId == _accountService.CurrentAccount.DefaultCreditCard.CreditCardId)
							{
								cardNumber += this.Services().Localize["DefaultCreditCard_Label"];
								creditCardInfos.IsDefault = true;
							}
							creditCardInfos.CardNumber = cardNumber;

							return creditCardInfos;

						}).OrderByDescending(cc => cc.CreditCardId == _accountService.CurrentAccount.DefaultCreditCard.CreditCardId).ToList();
			}
			catch(Exception e)
			{
				Logger.LogError(e);
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
				return CreditCards != null && CreditCards.Count < _appSettings.Data.MaxNumberOfCardsOnFile;
			}
		}

        public bool CanChooseTip
        {
            get
            {
				return PaymentSettings.IsPayInTaxiEnabled || PaymentSettings.PayPalClientSettings.IsEnabled;
            }
        }

		public ICommand NavigateToDetails
		{
			get
			{
				return this.GetCommand<CreditCardInfos>( cci =>
					{
						ShowViewModel<CreditCardAddViewModel>(new {creditCardId = cci.CreditCardId, isFromCreditCardListView = true, hasPaymentToSettle = _hasPaymentToSettle});
					});
			}
		}

        public ICommand NavigateToAddCard
        {
            get
            {
                return this.GetCommand(() =>
                    {
						ShowViewModel<CreditCardAddViewModel>(new {isAddingNew = true, isFromCreditCardListView = true, hasPaymentToSettle = _hasPaymentToSettle });
                    });
            }
        }
    }
}
