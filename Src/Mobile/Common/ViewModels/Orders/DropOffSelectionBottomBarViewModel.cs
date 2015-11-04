using System;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
	public class DropOffSelectionBottomBarViewModel : BaseViewModel
	{
		public DropOffSelectionBottomBarViewModel()
		{
		}

		public ICommand Cancel
		{
			get
			{
				return this.GetCommand(() =>
					{
						((HomeViewModel)Parent.Parent).CurrentViewState = HomeViewModelState.BookingStatus;
					});
			}
		}

		public ICommand SaveDropOff
		{
			get
			{
				return this.GetCommand(() =>
					{
						//need endpoint to update destination
					});
			}
		}
	}
}

