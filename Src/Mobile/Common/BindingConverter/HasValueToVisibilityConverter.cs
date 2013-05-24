using System;
using Cirrious.MvvmCross.Converters;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using Cirrious.MvvmCross.Interfaces.Localization;
using System.Collections;
using Cirrious.MvvmCross.Converters.Visibility;
using System.Globalization;

namespace BindingConverter
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

