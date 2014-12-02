using System;
using System.Windows.Input;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class PromotionItemViewModel : SelectableItemViewModel<ActivePromotion>
    {
        public PromotionItemViewModel(ActivePromotion activePromotion, ICommand promotionSelectedCommand)
            : base(activePromotion, promotionSelectedCommand)
        {
            Name = activePromotion.Name;
            Description = activePromotion.Description;
            ExpiringSoonWarning = GenerateExpiringSoonWarning(activePromotion.ExpirationDate);
            PromotionSelectedCommand = promotionSelectedCommand;
        }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public string ExpiringSoonWarning { get; private set; }

        public ICommand PromotionSelectedCommand { get; private set; }

        private string GenerateExpiringSoonWarning(DateTime? expirationDate)
        {
            const int thresholdForWarning = 4; // In days

            if (expirationDate == null)
            {
                return string.Empty;
            }

            var daysRemaining = (expirationDate.Value - DateTime.Now).Days;
            if (daysRemaining >= thresholdForWarning)
            {
                return string.Empty;
            }
            if (daysRemaining > 1 && daysRemaining < thresholdForWarning)
            {
                return string.Format(this.Services().Localize["PromoExpiringSoonMessage"], daysRemaining);
            }
            if (daysRemaining == 1)
            {
                return this.Services().Localize["PromoExpiringIn1DayMessage"];
            }
            return this.Services().Localize["PromoExpiringToday"];
        }
    }
}