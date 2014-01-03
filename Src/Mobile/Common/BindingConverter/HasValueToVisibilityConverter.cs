using System.Collections;
using System.Globalization;
using Cirrious.MvvmCross.Converters.Visibility;

namespace apcurium.MK.Booking.Mobile.BindingConverter
{
	public class HasValueToVisibilityConverter: MvxBaseVisibilityConverter
	{
		public override MvxVisibility ConvertToMvxVisibility(object value, object parameter, CultureInfo culture)
		{  
			if(value == null)
			{
				return MvxVisibility.Collapsed;               
			}
			var enumerable = value as IEnumerable;
			if(enumerable != null)
			{
				if(!enumerable.GetEnumerator().MoveNext())
				{
					return MvxVisibility.Collapsed;
				}
			}
			return MvxVisibility.Visible;

		}


	}
}

