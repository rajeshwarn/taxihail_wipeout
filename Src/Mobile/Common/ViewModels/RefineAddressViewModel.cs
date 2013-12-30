namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class RefineAddressViewModel : BaseSubViewModel<RefineAddressViewModel>
    {
        public RefineAddressViewModel(string messageId, string apt, string ringCode, string buildingName)
			:base(messageId)
        {
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
                FirePropertyChanged(() => StreetNumber);
            }
        }

        private string _streetAddress;
        public string StreetAddress
        {
            get { return _streetAddress; }
            set
            {
                _streetAddress = value;
                FirePropertyChanged(() => StreetAddress);
            }
        }

        private string _aptNumber;
        public string AptNumber
        {
            get { return _aptNumber; }
            set
            {
                _aptNumber = value;
                FirePropertyChanged(() => AptNumber);
            }
        }

        private string _ringCode;
        public string RingCode
        {
            get { return _ringCode; }
            set
            {
                _ringCode = value;
                FirePropertyChanged(() => RingCode);
            }
        }

        private string _buildingName;
        public string BuildingName
        {
            get { return _buildingName; }
            set
            {
                _buildingName = value;
                FirePropertyChanged(() => BuildingName);
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