using apcurium.MK.Booking.Mobile.Infrastructure;
using TinyIoC;

namespace apcurium.MK.Booking.Mobile.Client.ListViewStructure
{
    public class ListItemData
    {
        private static ListItemData _nullListItemData;

        public static ListItemData NullListItemData
        {
            get
            {
                return _nullListItemData ??
                       (_nullListItemData = new ListItemData
                       {
                           Key = int.MinValue,
                           Value = TinyIoCContainer.Current.Resolve<ILocalization>()["NoPreference"]
                       });
            }
        }

		public static int SpinnerEmptyItemKey = int.MinValue + 1;

		public static ListItemData SpinnerEmptyItem
		{
			get
			{ 
				return new ListItemData 
				{
					Key = int.MinValue + 1
				};
			}
		}

        public string Value { get; set; }

        public int Key { get; set; }

        public string Image { get; set; }

        public override string ToString()
        {
            return Value;
        }
    }
}