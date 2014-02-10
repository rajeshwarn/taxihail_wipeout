using System;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
	public enum HomeViewModelState
    {
		/// <summary>
        ///  Initial view state, with map
		/// </summary>
		Initial,
        /// <summary>
		/// Choose pickup date and time
        /// </summary>
        PickDate,
		/// <summary>
		/// Review order before confirming it
		/// </summary>
		Review,
		/// <summary>
		/// Edit some settings (Name, Passengers, Apartment...)
		/// </summary>
		Edit
    }
}

