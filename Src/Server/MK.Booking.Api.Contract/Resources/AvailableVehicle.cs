using ServiceStack.DataAnnotations;

namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class AvailableVehicle : BaseDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double VehicleNumber { get; set; }
        public string LogoName { get; set; }
    }

    public class AvailableVehicleCluster : AvailableVehicle
    {
    }
}