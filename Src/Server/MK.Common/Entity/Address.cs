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

        private string FirstSectionOfDisplayAddress
        {
            get
            {
                var firstSection = string.Join(" ", IsReversedFormat() 
                    ? new[] { Street.ToSafeString(), StreetNumber.ToSafeString() } 
                    : new[] { StreetNumber.ToSafeString(), Street.ToSafeString() });

                if (!firstSection.HasValueTrimmed())
                {
                    firstSection = FullAddress;
                }

                return firstSection;
            }
        }

        public string LastSectionOfDisplayAddress
        {
            get
            {
                var lastSection = string.Join(" ", State.ToSafeString(), ZipCode.ToSafeString());
                if (!lastSection.HasValueTrimmed())
                {
                    lastSection = string.Empty;
                }

                return lastSection;
            }
        }

        private string ConcatAddressComponents(bool useBuildingName = false)
        {
            var addressSections =
                new[] { FirstSectionOfDisplayAddress, City.ToSafeString(), LastSectionOfDisplayAddress }.Where(x => x.HasValueTrimmed()).ToArray();

            if (FirstSectionOfDisplayAddress.HasValueTrimmed() 
                && LastSectionOfDisplayAddress.HasValueTrimmed() 
                && FirstSectionOfDisplayAddress.Contains(LastSectionOfDisplayAddress))
            {
                // special case where we only had a FullAddress that we added value to but we don't want to redo the loop again
                return FirstSectionOfDisplayAddress;
            }

            // should return ("StreetNumber Street" or "Street StreetNumber" or "FullAddress"), City, State ZipCode
            var address = string.Join(", ", addressSections);

            if (useBuildingName && BuildingName.HasValueTrimmed())
            {
                address = BuildingName + " - " + address;
            }

            // Check if full address is really a full address
            // If it doesn't contain the city, then we overwrite FullAddress with the value of DisplayAddress
            // We also check that the street doesn't contain the city, ie: "11000 Garden Grove Blvd, Garden Grove, CA 92843"
            if (FullAddress.HasValueTrimmed())
            {
                if (!FullAddress.Contains(City.ToSafeString()))
                {
                    FullAddress = address;
                }
                else
                {
                    if (Street.HasValueTrimmed() && Street.Contains(City.ToSafeString()))
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
				    if (!DisplayAddress.Contains(FriendlyName))
				    {
                        return DisplayAddress;
                    }
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

        /// <summary>
        /// Determines if the address should be displayed in reversed order (Mexico, Netherlands)
        /// </summary>
        /// <returns><c>true</c> if this instance is reversed format; otherwise, <c>false</c>.</returns>
        private bool IsReversedFormat()
        {
            var reversed = false;

            try
            {
                var indexOfStreetNumber = FullAddress.IndexOf(StreetNumber, StringComparison.InvariantCultureIgnoreCase);
                var indexOfStreet = FullAddress.IndexOf(Street, StringComparison.InvariantCultureIgnoreCase);

                if(indexOfStreet >= 0 && indexOfStreetNumber >= 0)
                {
                    // we succesfully found the 2 indexes in FullAddress
                    reversed = indexOfStreet < indexOfStreetNumber;
                }
            }
            catch
            {
            }

            return reversed;
        }
    }
}