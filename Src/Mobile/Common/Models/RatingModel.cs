using System;
using System.Windows.Input;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile.Models
{
    //todo remove unused private field
    public class RatingModel : BaseViewModel
    {
        public Guid RatingTypeId { get; set; }
        public string RatingTypeName { get; set; }
        public int Score { get; set; }
		        
        public bool MadSelected
        {
            get { return Score == 1; }
            set
            {
				RaisePropertyChanged();
            }
        }
        
        public bool UnhappySelected
        {
            get { return Score == 2; }
            set
            {
				RaisePropertyChanged();
            }
        }

		public bool NeutralSelected
        {
            get { return Score == 3; }
            set
			{
				RaisePropertyChanged();
            }
        }
        
        public bool HappySelected
        {
            get { return Score == 4; }
            set
			{                
				RaisePropertyChanged();
            }
        }

        
        public bool EcstaticSelected
        {
            get { return Score == 5; }
            set
            {
				RaisePropertyChanged();
            }
        }

		public RatingModel(bool canRate = false)
        {
			CanRate = canRate;
        }

        private enum RatingState { Mad = 1, Unhappy = 2, Neutral = 3, Happy = 4, Ecstatic = 5 }

        private void DeselectAllState()
        {
            EcstaticSelected = false;
            HappySelected = false;
            NeutralSelected = false;
            UnhappySelected = false;
            MadSelected = false;
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
                return this.GetCommand<object>(param => param.Maybe(tag =>
                    {
					RatingState state;
					if(CanRate
					   && param != null
					   && Enum.TryParse(param.ToString(), true, out state))
					{
						Score = (int)state;
						DeselectAllState();
						switch (state)
						{
							case RatingState.Mad:
						    	MadSelected = true;
						    	break;
							case RatingState.Unhappy:
						    	UnhappySelected = true;
						    	break;
							case RatingState.Neutral:
						    	NeutralSelected = true;
						     	break;
						 	case RatingState.Happy:
						     	HappySelected = true;
						     	break;
						 	case RatingState.Ecstatic:
						     	EcstaticSelected = true;
						     	break;
						}
					}
				}));
            }
        }
    }
}