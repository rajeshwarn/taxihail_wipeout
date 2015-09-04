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
		Edit,
		/// <summary>
        /// Addresss search (favorites, history, etc...)
		/// </summary>
		AddressSearch,
        /// <summary>
        /// Airport search
        /// </summary>
        AirportSearch,
        /// <summary>
        /// Train station search
        /// </summary>
        TrainStationSearch,
        /// <summary>
        /// Book a taxi dialog.
        /// </summary>
        BookATaxi,
        /// <summary>
        /// Choose pickup date and time for Airport booking
        /// </summary>
        AirportPickDate,
        
		/// <summary>
		/// Booking Status mode (replaces the old booking status page).
		/// </summary>
		BookingStatus,
        /// <summary>
        /// Airport Details selection
        /// </summary>
        AirportDetails,
		/// <summary>
		/// Addresss search (favorites, history, etc...)
		/// </summary>
		AirportAddressSearch

    }
}

