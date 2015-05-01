using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class PromotionViewModel : PageViewModel
    {
        private readonly IOrderWorkflowService _orderWorkflowService;
        private readonly IPromotionService _promotionService;
		private readonly IAccountService _accountService;

        public PromotionViewModel(IOrderWorkflowService orderWorkflowService, IPromotionService promotionService, IAccountService accountService)
        {
            _orderWorkflowService = orderWorkflowService;
            _promotionService = promotionService;
			_accountService = accountService;

            ActivePromotions = new ObservableCollection<PromotionItemViewModel>();
        }

        public override void OnViewLoaded()
        {
            base.OnViewLoaded();
            LoadActivePromotions();
			CheckCoFAvaialble();
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

        public ICommand ApplyPromotion
        {
            get
            {
                return this.GetCommand(() =>
                {
                    _orderWorkflowService.SetPromoCode(PromotionCode);
                    Close(this);
                });
            }
        }

		private bool _isCoFAvailable;
		public bool IsCoFAvailable {
			get 
			{
				return _isCoFAvailable;
			}
			set 
			{
				_isCoFAvailable = value;
				RaisePropertyChanged();
			}
		}
		
		public ICommand ToPayment
		{
			get
			{
				return this.GetCommand(() => 
				{
					ShowViewModel<CreditCardAddViewModel>(new { isFromPromotions = true });
				})
			}
		}

        public ICommand SelectPromotion
        {
            get
            {
                return this.GetCommand<ActivePromotion>(activePromotion =>
                {
                    _orderWorkflowService.SetPromoCode(activePromotion.Code);
                    Close(this);
                });
            }
        }

		private async void CheckCoFAvaialble()
		{
			try
			{
				//TODO: Find a better way to check for in app pay detection 
				var cof = await _accountService.GetCreditCard();

				IsCoFAvailable = cof != null;
			}
			catch(Exception ex) 
			{
				Logger.LogError(ex);
			}
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