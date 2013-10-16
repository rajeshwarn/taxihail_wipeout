namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class AvailableVehicle : BaseDTO
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double VehicleNumber { get; set; }
    }

    public class AvailableVehicleCluster : AvailableVehicle
    {

    }
}
