using apcurium.MK.Booking.Mobile.Infrastructure;
using Cirrious.MvvmCross.ViewModels;

namespace apcurium.MK.Booking.Mobile.IoC
{
	public class ViewModelLocator: MvxDefaultViewModelLocator
    {
		readonly IAnalyticsService _analytics;

		public ViewModelLocator(IAnalyticsService analytics)
		{
			_analytics = analytics;
		}

		protected override void CallCustomInitMethods(IMvxViewModel viewModel, IMvxBundle parameterValues)
		{
			base.CallCustomInitMethods(viewModel, parameterValues);

			_analytics.LogViewModel(viewModel.GetType().Name);
		}
    }
}

