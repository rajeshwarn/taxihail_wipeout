using System;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.Models
{
    public class RatingModel : BaseViewModel
    {
        public Guid RatingTypeId { get; set; }
        public string RatingTypeName { get; set; }
        
		public int Score { get; set; }
		        
        public bool ScoreASelected
        {
			get { return Score > 0; }
            set
            {
				RaisePropertyChanged();
            }
        }
        
        public bool ScoreBSelected
        {
			get { return Score > 1; }
            set
            {
				RaisePropertyChanged();
            }
        }

		public bool ScoreCSelected
        {
			get { return Score > 2; }
            set
			{
				RaisePropertyChanged();
            }
        }
        
        public bool ScoreDSelected
        {
			get { return Score > 3; }
            set
			{                
				RaisePropertyChanged();
            }
        }
        
        public bool ScoreESelected
        {
			get { return Score > 4; }
            set
            {
				RaisePropertyChanged();
            }
        }

		public RatingModel(bool canRate = false)
        {
			CanRate = canRate;
        }

        public enum RatingState { ScoreA = 1, ScoreB = 2, ScoreC = 3, ScoreD = 4, ScoreE = 5 }

		private void UpdateButtonsState()
		{
			RaisePropertyChanged ("ScoreASelected");
			RaisePropertyChanged ("ScoreBSelected");
			RaisePropertyChanged ("ScoreCSelected");
			RaisePropertyChanged ("ScoreDSelected");
			RaisePropertyChanged ("ScoreESelected");
		}

		private bool _canRate;
		public bool CanRate
        {
            get
            {
				return _canRate;
            }
            set
			{
				_canRate = value;
				RaisePropertyChanged();
			}
        }

        public ICommand SetRateCommand
        {
            get
            {
				return this.GetCommand<object> (param => param.Maybe (tag => {
					if (!CanRate)
					{
						return;
					}

					RatingState state;
					Score = 0;
					if (param != null && Enum.TryParse (param.ToString (), true, out state)) {
						Score = (int)state;
					}
					UpdateButtonsState ();
				}));
            }
        }
    }
}