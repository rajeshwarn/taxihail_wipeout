using System;

using apcurium.MK.Booking.Mobile.AppServices;
using TinyIoC;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Extensions;
namespace apcurium.MK.Booking.Mobile.Data
{
    public class BookingInfoData
    {
        
        private Address _pickupLocation;
        private Address _destinationLocation;


        public BookingInfoData()
        {
            PickupLocation = new Address();
            DestinationLocation = new Address();
            Settings = new BookingSettings();
        }

        public Address PickupLocation
        {
            get { return _pickupLocation; }
            set { _pickupLocation = value; }
        }

        public Address DestinationLocation
        {
            get { return _destinationLocation; }
            set { _destinationLocation = value; }
        }



        public DateTime? PickupDate { get; set; }

        public int Id { get; set; }

        public DateTime? RequestedDateTime { get; set; }

        public BookingSettings Settings { get; set; }

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




         

        public DirectionInfo  GetDirectionInfo()
        {
            var result = new DirectionInfo();

            if (PickupLocation.HasValidCoordinate() && DestinationLocation.HasValidCoordinate())
            {
                result = TinyIoCContainer.Current.Resolve<IGeolocService>().GetDirectionInfo(PickupLocation.Longitude, PickupLocation.Latitude, DestinationLocation.Longitude, DestinationLocation.Latitude);
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

