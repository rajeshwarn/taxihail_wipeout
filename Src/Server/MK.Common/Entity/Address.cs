using System;
using apcurium.MK.Common.Extensions;
using System.Linq;

namespace apcurium.MK.Common.Entity
{
    public class Address
    {
        public Guid Id { get; set; }

        public string PlaceReference { get; set; }

        public string FriendlyName { get; set; }

        public string StreetNumber { get; set; }

        public string Street { get; set; }

        public string City { get; set; }

        public string ZipCode { get; set; }

        public string State { get; set; }

        public string FullAddress{ get; set; }

        public double Longitude { get; set; }

        public double Latitude { get; set; }

        public string Apartment { get; set; }

        public string RingCode { get; set; }

        public string BuildingName { get; set; }

        public bool IsHistoric { get; set; }

        public bool Favorite { get; set; }

        public string AddressType { get; set; }

        public string BookAddress
        {
            get
            {
                return ConcatAddressComponents();
            }
        }

        string ConcatAddressComponents ()
        {
            var prefixAddress = StreetNumber;
            if (BuildingName.HasValue ()) {
                prefixAddress = BuildingName;
            }
            var components = Params.Get (prefixAddress, Street, City, string.Format ("{0} {1}", State, ZipCode)).Where (s => s.HasValue () && s.Trim().HasValue()).ToList ();
            if (components.Count > 1) {
                return components.FirstOrDefault () + components.Skip (1).Aggregate ((x,y) => {
                    return string.Format (" {0}, {1}", x.Trim (), y.Trim ());});
            } else {
                return FullAddress;
            }
        }

        public void UpdateStreetOrNumberBuildingName (string streetNumberBuildingName)
        {
            if(streetNumberBuildingName.HasValue ()) {
                
                if(streetNumberBuildingName.IsDigit())
                {
                    StreetNumber = streetNumberBuildingName;
                    BuildingName = null;
                }
                else
                {
                    BuildingName = streetNumberBuildingName;
                }
            }
        }

        public void Copy (Address address)
        {
            if(address == null) return;
            address.FullAddress = this.FullAddress;
            address.Longitude = this.Longitude;
            address.Latitude = this.Latitude;
            address.Apartment = this.Apartment;
            address.RingCode = this.RingCode;
            address.BuildingName = this.BuildingName;
            address.Street = this.Street;
            address.StreetNumber = this.StreetNumber;
            address.City = this.City;
            address.ZipCode = this.ZipCode;
            address.State = this.State;
            address.PlaceReference = this.PlaceReference;
        }
    }
}