using System;
using System.Globalization;
using System.Linq;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using Cirrious.CrossCore.Converters;
using Cirrious.CrossCore.UI;
using Cirrious.MvvmCross.Plugins.Visibility;

namespace apcurium.MK.Booking.Mobile.BindingConverter
{
	public class HomeViewStateToVisibilityConverter : MvxBaseVisibilityValueConverter
	{
		protected override MvxVisibility Convert(object value, object parameter, CultureInfo culture)
		{
			var typedValue = value as HomeViewModelState?;
			var typedParameter = parameter as HomeViewModelState[];

			if (typedParameter == null || !typedValue.HasValue)
			{
				return MvxVisibility.Collapsed;
			}

			return typedParameter.Any(p => p == typedValue.Value)
				? MvxVisibility.Visible
				: MvxVisibility.Collapsed;
		}

	}
}