using System;
using Cirrious.MvvmCross.Commands;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.ViewModels;


namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class BookViewModel: MvxViewModel
    {
		private string _pickupLocation;
		private string _destinationLocation;
		public enum LocationType { Pickup, Destination }
		private LocationType _currentLocationType;

        public BookViewModel()
        {

        }

        public string PickupLocation
        {
            get{ return _pickupLocation;}
            set
            { 
                _pickupLocation = value;
                PickupLocationChanged.RaiseCanExecuteChanged();
            }
        }

		public string DestinationLocation
        {
            get{ return _destinationLocation;}
            set
            { 
                _destinationLocation = value;
                PickupLocationChanged.RaiseCanExecuteChanged();
            }
        }


		public LocationType CurrentLocationType {
			get { return _currentLocationType; }
			set { _currentLocationType = value; }
		}


		private MvxRelayCommand _requestCurrentLocationCommand;
        public MvxRelayCommand RequestCurrentLocationCommand
        {                   
            get
            {       
                if (_requestCurrentLocationCommand == null)
                {
                    _requestCurrentLocationCommand = new MvxRelayCommand(() => 
                    {       


                    } );
                }
                return _requestCurrentLocationCommand;
            }
        }

        private MvxRelayCommand _pickupLocationChanged;
        public MvxRelayCommand PickupLocationChanged
        {                   
            get
            {       
                if (_pickupLocationChanged == null)
                {
                    _pickupLocationChanged = new MvxRelayCommand(() => 
                    {       


                    } );
                }
                return _pickupLocationChanged;
            }
        }

    }
}

