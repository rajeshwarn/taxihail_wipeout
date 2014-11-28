using System.Collections.ObjectModel;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;

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

        public ObservableCollection<ActivePromotion> ActivePromotions { get; set; }

        private async void LoadActivePromotions()
        {
            ActivePromotions = new ObservableCollection<ActivePromotion>();

            var activePromotions = await _promotionService.GetActivePromotions();

            foreach (var activePromotion in activePromotions)
            {
                ActivePromotions.Add(activePromotion);
            }
        }

        private void SelectPromotion(ActivePromotion selectedPromotion)
        {
            _orderWorkflowService.SetPromoCode(selectedPromotion.Code);
            ShowViewModel<HomeViewModel>();
        }
    }
}