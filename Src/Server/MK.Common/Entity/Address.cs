using System;
using apcurium.MK.Common.Extensions;
using System.Linq;

namespace apcurium.MK.Common.Entity
{
    public class Address
    {
        private string fullStreeetWithoutNumber;

        public Guid Id { get; set; }

        public string PlaceReference { get; set; }

        public string FriendlyName { get; set; }

        public string StreetNumber { get; set; }

        public string Street { get; set; }

        public string City { get; set; }

        public string ZipCode { get; set; }

        string fullAddress;
        public string FullAddress {
            get {
                return fullAddress;
            }
            set {
                fullAddress = value;
                UpdateAddressComponents(value);
            }
        }

        public string FullAddressDisplay {
            get 
            {
                var prefixAddress = string.Empty;
                if(StreetNumber.HasValue())
                {
                    prefixAddress = StreetNumber + " " + prefixAddress;
                }

                if(BuildingName.HasValue())
                {
                    prefixAddress = BuildingName + " - " + prefixAddress;
                }

                return prefixAddress + fullStreeetWithoutNumber;
            }
        }

        public double Longitude { get; set; }

        public double Latitude { get; set; }

        public string Apartment { get; set; }

        public string RingCode { get; set; }

        public string BuildingName { get; set; }

        public bool IsHistoric { get; set; }

        public bool Favorites { get; set; }

        public string AddressType { get; set; } 

        void UpdateAddressComponents (string fullAddress)
        {
            if (fullAddress.HasValue ()) {

                var fullStreeetSplit = fullAddress.Split(' ');
                if (fullStreeetSplit.Length > 1)
                {
                    string newStreetNumber = fullStreeetSplit[0];

                    if(newStreetNumber.Contains("-"))
                    {
                        newStreetNumber = newStreetNumber.Split('-')[0].Trim();
                    }
                    if(string.IsNullOrEmpty(StreetNumber)) StreetNumber = newStreetNumber;

                    var newStreetName = fullStreeetWithoutNumber = fullStreeetSplit.Skip(1).Aggregate((x,y) => x + " " + y);
                    if(newStreetName.Contains(","))
                    {
                        newStreetName = newStreetName.Split(',')[0].Trim();
                    }
                    if(string.IsNullOrEmpty(Street)) Street = newStreetName;

                }
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
                    StreetNumber = null;
                    BuildingName = streetNumberBuildingName;
                }
            }
        }
    }
}