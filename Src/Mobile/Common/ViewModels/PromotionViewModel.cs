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

        public PromotionViewModel(IOrderWorkflowService orderWorkflowService, IPromotionService promotionService)
        {
            _orderWorkflowService = orderWorkflowService;
            _promotionService = promotionService;

            ActivePromotions = new ObservableCollection<PromotionItemViewModel>();
        }

        public override void OnViewLoaded()
        {
            base.OnViewLoaded();
            LoadActivePromotions();
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

        private async void LoadActivePromotions()
        {
            using (this.Services().Message.ShowProgress())
            {
                ActivePromotions.Clear();

                var activePromotions = await _promotionService.GetActivePromotions();

                foreach (var activePromotion in activePromotions)
                {
                    ActivePromotions.Add(new PromotionItemViewModel(activePromotion, SelectPromotion));
                }

                RaisePropertyChanged(() => ActivePromotions);
            }
        }
    }
}