using System;
using System.Linq;
using apcurium.MK.Common.Extensions;

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

        public string FullAddress { get; set; }

        public double Longitude { get; set; }

        public double Latitude { get; set; }

        public string Apartment { get; set; }

        public string RingCode { get; set; }

        public string BuildingName { get; set; }

        public bool IsHistoric { get; set; }

        public bool Favorite { get; set; }

        public string AddressType { get; set; }

        /// <summary>
        ///     This represents the address sent to the dispatch system.
        ///     The dispatch system has length constraints for addresses. The address must be short and
        ///     the most important information must appear first in the address (StreetNumber, Street and City)
        /// </summary>
        public string BookAddress
        {
            get { return ConcatAddressComponents(); }
        }

        /// <summary>
        ///     This represents the address displayed to the user in the application
        /// </summary>
        public string DisplayAddress
        {
            get { return ConcatAddressComponents(true); }
        }

        private string ConcatAddressComponents(bool useBuildingName = false)
        {
            var components =
                new[] {StreetNumber, Street, City, State, ZipCode}.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            if ((components.Length > 1) && (StreetNumber.HasValue()) && (Street.HasValue()))
            {
                // StreetNumber Street, City, State ZipCode
                var address = string.Join(", ", new[]
                {
                    string.Join(" ", new[] {StreetNumber, Street}),
                    City,
                    string.Join(" ", new[] {State, ZipCode})
                });

                if (useBuildingName && !string.IsNullOrWhiteSpace(BuildingName))
                {
                    address = BuildingName + " - " + address;
                }
                return address;
            }
            return FullAddress;
        }

        public void UpdateStreetOrNumberBuildingName(string streetNumberBuildingName)
        {
            if (streetNumberBuildingName.HasValue())
            {
                if (streetNumberBuildingName.IsDigit())
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

        public string GetFirstPortionOfAddress()
        {
            if ( (DisplayAddress.HasValue()) && ( DisplayAddress.Contains( "," ) ) )
            {
                return DisplayAddress.Split(',').First();
            }
            else
            {
                return DisplayAddress;
            }
        }

        /// <summary>
        ///     Returns a MemberwiseClone of the Address
        /// </summary>
        public Address Copy()
        {
            return (Address)this.MemberwiseClone();
        }

        /// <summary>
        ///     Copies only the location-related fields without changing the rest (for example 'FriendlyName', 'Apartment', 'RingCode' and 'IsHistory')
        /// </summary>
        public void CopyLocationInfoTo(Address address)
        {
            if (address == null) return;

            address.FullAddress = FullAddress;
            address.Longitude = Longitude;
            address.Latitude = Latitude;
            address.BuildingName = BuildingName;
            address.Street = Street;
            address.StreetNumber = StreetNumber;
            address.City = City;
            address.ZipCode = ZipCode;
            address.State = State;
            address.PlaceReference = PlaceReference;
        }
    }
}