namespace CustomerPortal.Contract.Resources
{
    public class MapCoordinate
    {
        private readonly double _latitude;
        private readonly double _longitude;
        public double Latitude { get { return _latitude; } }
        public double Longitude { get { return _longitude; } }

        public MapCoordinate(double latitude, double longitude)
        {
            _latitude = latitude;
            _longitude = longitude;
        }
        public override string ToString()
        {
            return string.Format("{0},{1}", Latitude, Longitude);
        }
    }
}