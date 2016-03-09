using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using apcurium.MK.Booking.Mobile.Data;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Linq;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class EditAutoTipViewModel : PageViewModel, ISubViewModel<int>
    {
        private readonly IOrderWorkflowService _orderWorkflowService;
        private readonly IPaymentService _paymentService;
        private readonly IBookingService _bookingService;
        private readonly IAccountService _accountService;
		private List<CreditCardInfos> _creditCardsData;
		private bool _isCmtRideLinq;
        private const int TIP_MAX_PERCENT = 100;
		private const string Visa = "Visa";
		private const string MasterCard = "MasterCard";
		private const string Amex = "Amex";
		private const string CreditCardGeneric = "Credit Card Generic";
		private const string VisaElectron = "Visa Electron";
		private const string Discover = "Discover";
		private List<ListItem> _creditCardCompanies;

        public EditAutoTipViewModel(IOrderWorkflowService orderWorkflowService,
            IPaymentService paymentService,
            IAccountService accountService,
            IBookingService bookingService)
        {
            _orderWorkflowService = orderWorkflowService;
            _paymentService = paymentService;
            _bookingService = bookingService;
			_accountService = accountService;

			GetIsCmtRideLinq();
		}

		private async void GetIsCmtRideLinq()
		{
			try
			{
				var paymentSettings = await _paymentService.GetPaymentSettings();

				_isCmtRideLinq = paymentSettings.PaymentMode == PaymentMethod.RideLinqCmt;

				RaisePropertyChanged(() => ViewTitle);
				RaisePropertyChanged(() => CanShowCreditCard);
			}
			catch(Exception ex) 
			{
				Logger.LogError(ex);	
			}
		}

		public async void Init(int tip = -1)
		{
			if (_paymentPreferences == null)
			{
				_paymentPreferences = Container.Resolve<PaymentDetailsViewModel>();
				await _paymentPreferences.Start();
			}
			if (tip > -1)
			{
				_paymentPreferences.Tip = tip;
			}
			PaymentPreferences = _paymentPreferences;
        }

        public override async void OnViewStarted(bool firstTime)
        {
            base.OnViewStarted(firstTime);

            if (firstTime)
            {
				_creditCardCompanies = new List<ListItem>
					{
						new ListItem {Display = Visa, Image = "visa"},
						new ListItem {Display = MasterCard, Image = "mastercard"},
						new ListItem {Display = Amex, Image = "amex"},
						new ListItem {Display = VisaElectron, Image = "visa_electron"},
						new ListItem {Display = Discover, Image =  "discover"},
						new ListItem {Display = CreditCardGeneric, Image =  "credit_card_generic"}
					};

                using (this.Services().Message.ShowProgress())
                {
                    await GetCreditCards();
					CreditCardSelected = CreditCards.First(cc => cc.IsDefault.Value).Id.Value;
                }   
            }
            else
            {
                await GetCreditCards();
            }

        }

		private PaymentDetailsViewModel _paymentPreferences;
		public PaymentDetailsViewModel PaymentPreferences 
		{ 
			get{ return _paymentPreferences;} 
			private set 
			{ 
				_paymentPreferences = value; 
				RaisePropertyChanged(); 
			}
        }

        private ListItem[] _creditCards;
        public ListItem[] CreditCards
        {
            get
            {
                return _creditCards;
            }
            set
            {
                _creditCards = value;
                RaisePropertyChanged();
            }
        }

        public string CreditCardSelectedDisplay
        {
            get
            {
                return CreditCards[CreditCardSelected].Display;
            }
        }

        public string CreditCardSelectedImage
        {
            get
            {
				return CreditCards[CreditCardSelected].Image;
            }
		}

		public string ViewTitle
		{
			get
			{
				return _isCmtRideLinq ? this.Services().Localize["View_EditAutoPayment"] : this.Services().Localize["View_EditAutoTip"];
			}
		}

		public bool CanShowCreditCard
		{
			get
			{
				return _isCmtRideLinq;
			}
		}

		public bool CanChangeCreditCard
		{
			get
			{
				return this.Settings.ChangeCreditCardMidtrip;
			}
		}

        private int _creditCardSelected;
        public int CreditCardSelected
        {
            get
            {
                return _creditCardSelected;
            }
            set
            {
                _creditCardSelected = value;
                RaisePropertyChanged();
                RaisePropertyChanged(() => CreditCardSelectedDisplay);
                RaisePropertyChanged(() => CreditCardSelectedImage);
            }
        }

        private async Task GetCreditCards()
        {
            try
            {
                var creditCardsDetails = await _accountService.GetCreditCards();

				_creditCardsData = creditCardsDetails.Select( cc => 
	                {
							var creditCardInfos =  new CreditCardInfos()
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

                CreditCards = _creditCardsData.Select(cc =>
                    {
                        return new ListItem()
                        {
                            Id = _creditCardsData.FindIndex(c => c == cc),
                            Display = cc.CardNumber,
							IsDefault = cc.IsDefault,
							Image = _creditCardCompanies.FirstOrDefault(c => c.Display == cc.CreditCardCompany).Image
                        };
                    }).ToArray();
            }
            catch(Exception e)
            {
                Logger.LogError(e);
            }
        }

        public ICommand SaveAutoTipChangeCommand
        {
            get
            {
                return this.GetCommand(async () =>
                {
                    using (this.Services().Message.ShowProgress())
                    {
						if(PaymentPreferences.Tip > TIP_MAX_PERCENT)
						{
							await this.Services().Message.ShowMessage(null, this.Services().Localize["TipPercent_Error"]);
						}
						else
						{
	                        var activeOrder = await _orderWorkflowService.GetLastActiveOrder();
	                        if (activeOrder != null)
	                        {
								var autoTipUpdated = activeOrder.Order.IsManualRideLinq
									? await _bookingService.UpdateAutoTipForManualRideLinq(activeOrder.Order.Id, PaymentPreferences.Tip) 
									: await _paymentService.UpdateAutoTip(activeOrder.Order.Id, PaymentPreferences.Tip);

								if (autoTipUpdated)
								{
									this.ReturnResult(PaymentPreferences.Tip);
								}
								else
								{
									this.Services().Message
										.ShowMessage(this.Services().Localize["Error_EditAutoTipTitle"], this.Services().Localize["Error_EditAutoTipMessage"])
										.FireAndForget();
								}
	                        }
						}
                    }
                });
            }
        }
    }
}