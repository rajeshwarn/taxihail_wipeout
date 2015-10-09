using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class PromotionViewModel : PageViewModel
    {
        private readonly IOrderWorkflowService _orderWorkflowService;
        private readonly IPromotionService _promotionService;
		private readonly IAccountService _accountService;
		private readonly IPaymentService _paymentService;

		private ClientPaymentSettings _paymentSettings;

		public PromotionViewModel(IOrderWorkflowService orderWorkflowService, IPromotionService promotionService, IAccountService accountService, IPaymentService paymentService)
        {
            _orderWorkflowService = orderWorkflowService;
            _promotionService = promotionService;
			_accountService = accountService;
			_paymentService = paymentService;

            ActivePromotions = new ObservableCollection<PromotionItemViewModel>();
        }

        public override void OnViewLoaded()
        {
            base.OnViewLoaded();
			FetchPaymentSettings();
            LoadActivePromotions();
        }

        public override async void OnViewStarted(bool firstTime)
        {
            base.OnViewStarted(firstTime);

            var creditCard = await _accountService.GetDefaultCreditCard();

            HasValidPaymentInformation = !(creditCard == null || creditCard.IsExpired() || creditCard.IsDeactivated);
        }

        private string _promotionCode;
        public string PromotionCode
        {
            get { return _promotionCode; }
            set
            {
                if (_promotionCode != value)
                {
                    _promotionCode = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ObservableCollection<PromotionItemViewModel> ActivePromotions { get; set; }

        private bool _hasValidPaymentInformation;
		public bool HasValidPaymentInformation 
		{
			get { return _hasValidPaymentInformation; }
			set 
			{
				_hasValidPaymentInformation = value;
				RaisePropertyChanged();
                RaisePropertyChanged(() => PromoRequiresPaymentMethodText);
			}
		}

        public string PromoRequiresPaymentMethodText
        {
            get
            {
				if (HasValidPaymentInformation)
				{
					return this.Services().Localize["PromoMustUseCardOnFileWarningMessage"];
				}

				return GetNoValidPaymentInformationMessage();
            }
        }

        public ICommand ToPayment
		{
			get
			{
			    return this.GetCommand(() =>
			    {
			        ShowViewModel<CreditCardAddViewModel>(new {isFromPromotionsView = true});
			    });
			}
		}

        public ICommand SelectPromotion
        {
            get
            {
				return this.GetCommand<ActivePromotion>(activePromotion => SetPromoCodeAndClose(activePromotion.Code));
            }
        }

		public ICommand ApplyPromotion
		{
			get
			{
				return this.GetCommand(() => SetPromoCodeAndClose(PromotionCode));
			}
		}

		private string GetNoValidPaymentInformationMessage()
		{
			switch (_paymentSettings.SupportedPaymentMethod)
			{
				case SupportedPaymentMethod.Multiple:
					return this.Services().Localize["PromoMustHavePaymentMethodSetMessage_Multiple"];
				case SupportedPaymentMethod.PayPalOnly:
					return this.Services().Localize["PromoMustHavePaymentMethodSetMessage_PayPal"];
				default:
					return this.Services().Localize["PromoMustHavePaymentMethodSetMessage_CreditCard"];
			}
		}

		private void SetPromoCodeAndClose(string promoCode)
		{
			if (!HasValidPaymentInformation)
			{
				this.Services().Message.ShowMessage(
					this.Services().Localize["Error"],
					GetNoValidPaymentInformationMessage());

				return;
			}

			_orderWorkflowService.SetPromoCode(promoCode);
			Close(this);
		}

		public string AddPaymentMethodButtonTitle
		{
			get
			{
				switch (_paymentSettings.SupportedPaymentMethod)
				{
					case SupportedPaymentMethod.Multiple:
						return this.Services().Localize["AddPaymentMethod_Multiple"];
					case SupportedPaymentMethod.PayPalOnly:
						return this.Services().Localize["AddPaymentMethod_PayPal"];
					default:
						return this.Services().Localize["AddPaymentMethod_CreditCard"];
				}
			}
		}

		private async void FetchPaymentSettings()
		{
			_paymentSettings = await _paymentService.GetPaymentSettings();
			RaisePropertyChanged(() => AddPaymentMethodButtonTitle);
		}

        private async void LoadActivePromotions()
        {
            using (this.Services().Message.ShowProgress())
            {
                ActivePromotions.Clear();

                try
                {
                    var activePromotions = await _promotionService.GetActivePromotions();

                    foreach (var activePromotion in activePromotions)
                    {
                        ActivePromotions.Add(new PromotionItemViewModel(activePromotion, SelectPromotion));
                    }

                    RaisePropertyChanged(() => ActivePromotions);
                }
                catch (Exception ex)
                {
                    Logger.LogMessage(ex.Message, ex.ToString());
                    this.Services().Message.ShowMessage(this.Services().Localize["Error"], this.Services().Localize["PromotionLoadError"]);
                }
            }
        }
    }
}