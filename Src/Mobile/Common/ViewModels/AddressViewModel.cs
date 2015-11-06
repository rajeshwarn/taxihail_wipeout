using System.Linq;
using apcurium.MK.Booking.Maps.Geo;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class AddressViewModel : BaseViewModel
	{
		public AddressViewModel()
		{
			
		}

		public AddressViewModel(Address address, AddressType type)
		{
			Address = address;
			Type = type;
		}

        public Address Address { get; set; }
        public bool ShowRightArrow { get; set; }
        public bool ShowPlusSign { get; set; }
        public bool IsAddNew { get; set; }

        private bool _isFirst;
        private bool _isLast;

        public string DisplayLine1
        {
            get
            {
				return Address.DisplayLine1;
            }
        }

        public string DisplayLine2
        {
            get
            {
				return Address.DisplayLine2;
            }
        }

		public string Icon
		{
			get
			{
				switch (Type)
				{
					case AddressType.Favorites:
						return "favorites";
					case AddressType.History:
						return "history";
					case AddressType.Places:
						return "places";						
					default:
						return null;
				}
			}
		}

        public bool IsFirst 
		{ 
			get { return _isFirst; } 
            set 
            {
                _isFirst = value;
				RaisePropertyChanged();
            }
        }

        public bool IsLast 
		{ 
			get { return _isLast; } 
            set 
            {
                _isLast = value;
				RaisePropertyChanged();
            }
        }

		public AddressType Type { get; set; }

		public bool IsSearchResult { get; set; }

		public Position ToPosition()
		{
			return this.Address != null ? 
						new Position(this.Address.Latitude, this.Address.Longitude)
						: new Position();
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hash = 17;
				hash = hash * 23 + DisplayLine1.ToSafeString().GetHashCode();
				hash = hash * 23 + DisplayLine2.ToSafeString().GetHashCode();
				hash = hash * 23 + Type.GetHashCode();
				return hash;
			}
		}

		public override bool Equals(object obj)
		{
			var other = (AddressViewModel)obj;

			var equal = (DisplayLine1 == other.DisplayLine1)
			            && (DisplayLine2 == other.DisplayLine2)
			            && (Type == other.Type);

			return equal;
		}
	}

	public enum AddressType
	{
		Favorites,
		Places,
		History
	}
}

