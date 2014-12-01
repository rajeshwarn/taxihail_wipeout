using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Windows.Input;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Extensions;
using ServiceStack.Text;

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

        public ICommand SelectPromotion
        {
            get
            {
                return this.GetCommand<ActivePromotion>(activePromotion =>
                {
                    _orderWorkflowService.SetPromoCode(activePromotion.Code);
                    ShowViewModel<HomeViewModel>();
                });
            }
        }

        private async void LoadActivePromotions()
        {
            ActivePromotions = new ObservableCollection<ActivePromotion>();

            var activePromotions = await _promotionService.GetActivePromotions();

            foreach (var activePromotion in activePromotions)
            {
                ActivePromotions.Add(activePromotion);
            }
        }
    }
}