namespace apcurium.MK.Common.Entity
{
    public class BookingSettings
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public int? ProviderId { get; set; }
        public int? VehicleTypeId { get; set; }
        public string VehicleType { get; set; }
        public int? ChargeTypeId { get; set; }
        public string ChargeType { get; set; }
        public string AccountNumber { get; set; }
        public int NumberOfTaxi { get; set; }
        public int LargeBags { get; set; }

        private int _passengers;
        public int Passengers 
        { 
            get { return _passengers <= 0 ? 1 : _passengers; }
            set
            {
                _passengers = value <= 0 ? 1 : value;
            }
        }

        public BookingSettings Copy()
        {
            return (BookingSettings)this.MemberwiseClone();
        }
    }
}