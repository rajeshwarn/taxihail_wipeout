using System;
using Cirrious.MvvmCross.ViewModels;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
	public class OrderReviewPresentationHint: MvxPresentationHint
    {
		public OrderReviewPresentationHint(bool show = true)
        {
			Show = show;
        }

		public bool Show
		{
			get;
			private set;
		}
    }
}

