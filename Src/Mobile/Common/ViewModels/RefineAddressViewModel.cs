namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class RefineAddressViewModel : BaseSubViewModel<RefineAddressViewModel>
    {
		public void Init(string messageId, string apt, string ringCode, string buildingName)
        {
			Init(messageId);

            AptNumber = apt;
            RingCode  = ringCode;
            BuildingName = buildingName;
        }

        private string _streetNumber;
        public string StreetNumber
        {
            get { return _streetNumber; }
            set
            {
                _streetNumber = value;
				RaisePropertyChanged();
            }
        }

        private string _streetAddress;
        public string StreetAddress
        {
            get { return _streetAddress; }
            set
            {
                _streetAddress = value;
				RaisePropertyChanged();
            }
        }

        private string _aptNumber;
        public string AptNumber
        {
            get { return _aptNumber; }
            set
            {
                _aptNumber = value;
				RaisePropertyChanged();
            }
        }

        private string _ringCode;
        public string RingCode
        {
            get { return _ringCode; }
            set
            {
                _ringCode = value;
				RaisePropertyChanged();
            }
        }

        private string _buildingName;
        public string BuildingName
        {
            get { return _buildingName; }
            set
            {
                _buildingName = value;
				RaisePropertyChanged();
            }
        }

        public AsyncCommand SaveCommand
        {
            get
            {

                return GetCommand(() => ReturnResult(this));
            }
        }
    }
}