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

        public ObservableCollection<PromotionItemViewModel> ActivePromotions { get; set; }

		public bool HasPromotions 
		{
			get 
			{
				return ActivePromotions.Count > 0;
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
            ActivePromotions = new ObservableCollection<PromotionItemViewModel>();

            var promotions = await _promotionService.GetActivePromotions();
            var activePromotions = promotions.Select(p => new PromotionItemViewModel(p, SelectPromotion));

            foreach (var activePromotion in activePromotions)
            {
                ActivePromotions.Add(activePromotion);
            }

			RaisePropertyChanged(() => HasPromotions);
        }
    }
}