namespace apcurium.MK.Booking.Domain
{
    public class BookingSettings
    {        
        public string Name { get; set; }
        public string Phone { get; set; }
        public int Passengers { get; set; }
        public int VehicleTypeId { get; set; }
        public int ChargeTypeId { get; set; }
        public int ProviderId { get; set; }
        public int NumberOfTaxi { get; set; } 
    }
}