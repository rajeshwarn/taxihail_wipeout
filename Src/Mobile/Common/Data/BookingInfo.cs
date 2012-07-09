using System;
using Microsoft.Practices.ServiceLocation;

namespace TaxiMobileApp
{
    public class BookingInfoData
    {
        private LocationData _pickupLocation;

        private LocationData _destinationLocation;


        public BookingInfoData()
        {
            PickupLocation = new LocationData();
            DestinationLocation = new LocationData();
            Settings = new BookingSetting();
        }

        public LocationData PickupLocation
        {
            get { return _pickupLocation; }
            set { _pickupLocation = value; }
        }

        public LocationData DestinationLocation
        {
            get { return _destinationLocation; }
            set { _destinationLocation = value; }
        }



        public DateTime? PickupDate { get; set; }

        public int Id { get; set; }

        public DateTime? RequestedDateTime { get; set; }

        public BookingSetting Settings { get; set; }

        public string Status { get; set; }

		public string Notes { get; set; }

        public bool Hide { get; set; }

        public BookingInfoData Copy()
        {

            var copy = new BookingInfoData();
            copy.PickupLocation = PickupLocation.Copy();
            copy.DestinationLocation = DestinationLocation.Copy();
            copy.PickupDate = null;
            copy.Status = "";
            copy.Settings = Settings.Copy();
            return copy;
        }


        public double? GetDistance()
        {
            double? result = null;

            if (PickupLocation.HasValidCoordinate() && DestinationLocation.HasValidCoordinate())
            {
                var service = ServiceLocator.Current.GetInstance<IBookingService>();
                result = service.GetRouteDistance(PickupLocation.Longitude.Value, PickupLocation.Latitude.Value, DestinationLocation.Longitude.Value, DestinationLocation.Latitude.Value);
            }

            return result;


        }


        private const double _callCost = 3.45;
        private const double _costPerKm = 1.70;

        public double? GetPrice(double? distance)
        {

            double? price = null;
            try
            {
                if (distance.HasValue && (distance.Value > 0))
                {
                    var km = (distance.Value / 1000);

                    if (km < 5)
                    {
                        price = _callCost + (km * _costPerKm) + (((km * _costPerKm) + _callCost) * 0.2);
                    }
                    else if (km < 50)
                    {
                        price = _callCost + (km * _costPerKm) + (((km * _costPerKm) + _callCost) * 0.2) + 2;
                    }
                    else
                    {
                        price = 1000;
                    }

                    if (price.HasValue)
                    {

                        price = Math.Round(price.Value, 2);

                        Console.WriteLine(price);

                        price = price.Value * 100;

                        int q = (int)(price.Value / 5.0);

                        int r;

                        Math.DivRem((int)price.Value, 5, out r);
                        Console.WriteLine(" r : " + r.ToString());
                        if (r > 0)
                        {
                            q++;
                        }

                        price = q * 5;

                        Console.WriteLine(price);

                        price = price.Value / 100;

                        Console.WriteLine(price);

                    }
                }
            }
            catch
            {
            }

            return price;
        }
    }
}

