using System;
using System.Linq;
using apcurium.MK.Common.Extensions;
using Newtonsoft.Json;
using MK.Common.Serializer;

namespace apcurium.MK.Common.Entity
{
    public class Address
    {
        public Guid Id { get; set; }

        public string PlaceId { get; set; }

        public string FriendlyName { get; set; }

        public string StreetNumber { get; set; }

        [JsonConverter(typeof(TolerantEnumConverter))]
        public AddressLocationType AddressLocationType { get; set; }

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
        ///     This represents the address displayed to the user in the application
        /// </summary>
        public string DisplayAddress
        {
            get { return ConcatAddressComponents(true); }
        }

        private string ConcatAddressComponents(bool useBuildingName = false)
        {
            var components =
                new[] { StreetNumber, Street, City, State, ZipCode }.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

            if ((components.Length <= 1) || !StreetNumber.HasValueTrimmed() || !Street.HasValueTrimmed() || !City.HasValueTrimmed())
            {
                return FullAddress;
            }

            // StreetNumber Street, City, State ZipCode
            var address = string.Join(", ", string.Join(" ", StreetNumber, Street), City, string.Join(" ", State, ZipCode));

            if (useBuildingName && BuildingName.HasValueTrimmed())
            {
                address = BuildingName + " - " + address;
            }

            // Check if full address is really a full address
            // If it doesn't contain the city, then we overwrite FullAddress with the value of DisplayAddress
            // We also check that the street doesn't contain the city, ie: "11000 Garden Grove Blvd, Garden Grove, CA 92843"
            if (FullAddress.HasValueTrimmed())
            {
                if (!FullAddress.Contains(City))
                {
                    FullAddress = address;
                }
                else
                {
                    if (Street.Contains(City))
                    {
                        FullAddress = address;
                    }
                }
            }

            return address;
        }

        public string GetFirstPortionOfAddress()
        {
            if ((DisplayAddress.HasValue()) && (DisplayAddress.Contains(",")))
            {
                return DisplayAddress.Split(',').First();
            }

            return DisplayAddress;
        }

        public void ChangeStreetNumber(string newStreetNumber)
        {
            if (StreetNumber.HasValue())
            {
                FullAddress = FullAddress.Replace(StreetNumber, newStreetNumber);
            }

            StreetNumber = newStreetNumber;
        }

		public string DisplayLine1
		{
			get
			{
				if (AddressType == "place" || FriendlyName.HasValue())
				{
					return FriendlyName;
				}

                if (DisplayAddress == null)
                {
                    return string.Empty;
                }

                var splitStrings = DisplayAddress.SplitOnFirst(",");

                if (splitStrings.Length > 0)
                {
                    return splitStrings[0];
                }

                return DisplayAddress;
			}
		}

		public string DisplayLine2
		{
			get
			{
				if (AddressType == "place" || FriendlyName.HasValue())
				{
					return DisplayAddress;
				}

                if (DisplayAddress == null)
                {
                    return string.Empty;
                }

                var splitStrings = DisplayAddress.SplitOnFirst(",");

                if (splitStrings.Length > 1)
                {
                    return splitStrings[1].TrimStart();
                }

                return string.Empty;
			}
		}

		/// <summary>
        ///     Returns a MemberwiseClone of the Address
        /// </summary>
        public Address Copy()
        {
            return (Address) this.MemberwiseClone();
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
            address.PlaceId = PlaceId;
        }

        public bool IsValid()
        {
            return FullAddress.HasValue()
                && Longitude != 0
                && Latitude != 0;
        }
    }
}