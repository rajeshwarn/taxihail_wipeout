using System.Linq;
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
        }

        public override void OnViewLoaded()
        {
            base.OnViewLoaded();
            LoadActivePromotions();
        }

        public ObservableCollection<SelectableItemViewModel<ActivePromotion>> ActivePromotions { get; set; }

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
            ActivePromotions = new ObservableCollection<SelectableItemViewModel<ActivePromotion>>();

            var promotions = await _promotionService.GetActivePromotions();
            var activePromotions = promotions.Select(p => new SelectableItemViewModel<ActivePromotion>(p, SelectPromotion));

            foreach (var activePromotion in activePromotions)
            {
                ActivePromotions.Add(activePromotion);
            }
        }
    }
}