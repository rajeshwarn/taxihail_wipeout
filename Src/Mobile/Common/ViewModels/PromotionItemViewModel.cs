using System;
using System.Windows.Input;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class PromotionItemViewModel : SelectableItemViewModel<ActivePromotion>
    {
        public PromotionItemViewModel(ActivePromotion activePromotion, ICommand redeemPromotion)
            : base(activePromotion, redeemPromotion)
        {
            Name = activePromotion.Name;
            Description = activePromotion.Description;
            ProgressDescription = GetProgressDescription(activePromotion);
            IsUnlocked = GetUnlockedStatus(activePromotion);
            ExpiringSoonWarning = GenerateExpiringSoonWarning(activePromotion.ExpirationDate);

            AddPromoCodeToDescription(activePromotion.Code);
        }

		public ICommand SelectPromotion
		{
			get
			{
				return this.GetCommand(() =>
				{
					IsExpanded = !IsExpanded;
				});
			}
		}

        public string Name { get; private set; }

        public string Description { get; private set; }

        public string ProgressDescription { get; private set; }
        
        public bool IsUnlocked { get; set; }

        public string ExpiringSoonWarning { get; private set; }

		private bool _isExpanded;
		public bool IsExpanded
		{
			get { return _isExpanded; }
			set
			{
				if (_isExpanded != value)
				{
					_isExpanded = value;
					RaisePropertyChanged();
				}
			}
		}

        private void AddPromoCodeToDescription(string code)
        {
            Description += string.Format("{0}{1}{2}",
                Environment.NewLine,
                Environment.NewLine,
                string.Format(this.Services().Localize["PromoDescriptionCode"], code.ToUpper()));
        }

        private string GetProgressDescription(ActivePromotion promotion)
        {
            return string.Empty;

            // feature on hold
            //if (!promotion.Progress.HasValue || !promotion.UnlockGoal.HasValue)
            //{
            //    return string.Empty;
            //}

            //return string.Format("{0} {1}/{2}", this.Services().Localize["PromoProgress"], promotion.Progress, promotion.UnlockGoal);
        }

        private bool GetUnlockedStatus(ActivePromotion promotion)
        {
            return true;

            // feature on hold
            //if (!promotion.Progress.HasValue || !promotion.UnlockGoal.HasValue)
            //{
            //    return true;
            //}

            //return promotion.Progress.Value >= promotion.UnlockGoal.Value;
        }

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