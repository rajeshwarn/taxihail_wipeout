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

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class EditAutoTipViewModel : PageViewModel, ISubViewModel<int>
    {
        private readonly IOrderWorkflowService _orderWorkflowService;
        private readonly IPaymentService _paymentService;
        private readonly IBookingService _bookingService;
        private readonly IAccountService _accountService;
        private List<CreditCardInfos> _creditCardsData;
        private const int TIP_MAX_PERCENT = 100;

        public EditAutoTipViewModel(IOrderWorkflowService orderWorkflowService,
            IPaymentService paymentService,
            IAccountService accountService,
            IBookingService bookingService)
        {
            _orderWorkflowService = orderWorkflowService;
            _paymentService = paymentService;
            _bookingService = bookingService;
            _accountService = accountService;
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
                var defaultCreditCard = creditCardsDetails.First(cc => cc.CreditCardId == _accountService.CurrentAccount.DefaultCreditCard.CreditCardId);
                var orderedCreditCards = creditCardsDetails.ToList();
                orderedCreditCards.Remove(defaultCreditCard);
                orderedCreditCards.Insert(0, defaultCreditCard);
                _creditCardsData = orderedCreditCards.Select( cc => 
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

                CreditCards = _creditCardsData.Select(cc =>
                    {
                        return new ListItem()
                        {
                            Id = _creditCardsData.FindIndex(c => c == cc),
                            Display = cc.CardNumber,
                            IsDefault = cc.CreditCardId == defaultCreditCard.CreditCardId,
                            Image = cc.CreditCardCompany
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
	                            var autoTipUpdated = activeOrder.Item1.IsManualRideLinq
									? await _bookingService.UpdateAutoTipForManualRideLinq(activeOrder.Item1.Id, PaymentPreferences.Tip) 
									: await _paymentService.UpdateAutoTip(activeOrder.Item1.Id, PaymentPreferences.Tip);

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