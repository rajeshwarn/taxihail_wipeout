using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface IOrderWorkflowService
	{
		Task PrepareForNewOrder();

		void BeginCreateOrder ();

		void EndCreateOrder ();

		Task<bool> ValidateCardOnFile ();
		Task<bool> ValidateCardExpiration ();
	    Task<bool> ValidateIsCardDeactivated();
		Task<bool> ValidatePromotionUseConditions();

		Task SetAddress(Address address);

		Task SetPickupAptAndRingCode(string apt, string ringCode);

		Task<Address> SetAddressToUserLocation(CancellationToken cancellationToken = default(CancellationToken));

		Task ClearDestinationAddress();

        Task SetAddressToCoordinate(Position coordinate, CancellationToken cancellationToken);

		Task SetPickupDate(DateTime? date);

		Task ToggleBetweenPickupAndDestinationSelectionMode();

		void SetAddressSelectionMode(AddressSelectionMode mode = AddressSelectionMode.None);

		Task ToggleIsDestinationModeOpened(bool? forceValue = null);

		Task ValidatePickupTime();

		Task ValidatePickupAndDestination();

		Task ValidateNumberOfPassengers (int? numberOfPassengers);

	    Task<bool> ValidateChargeType();

		Task<Tuple<Order, OrderStatusDetail>> ConfirmOrder();

		Task SetVehicleType (int? vehicleTypeId);

		Task SetBookingSettings(BookingSettings bookingSettings);

		Task SetAccountNumber (string accountNumber, string customerNumber);

		void SetNoteToDriver(string text);

        void SetTipIncentive(double? tipIncentive);

		void SetPromoCode(string code);

		Task<double?> GetTipIncentive();

		IObservable<Address> GetAndObservePickupAddress();

		IObservable<Address> GetAndObserveDestinationAddress();

		IObservable<AddressSelectionMode> GetAndObserveAddressSelectionMode();

		IObservable<PickupPoint[]> GetAndObservePOIRefPickupList();

        IObservable<Airline[]> GetAndObservePOIRefAirlineList();

        IObservable<int?> GetAndObserveVehicleType();

		IObservable<BookingSettings> GetAndObserveBookingSettings();

		IObservable<DateTime?> GetAndObservePickupDate();

		IObservable<string> GetAndObserveEstimatedFare();

		IObservable<string> GetAndObserveNoteToDriver();

        IObservable<double?> GetAndObserveTipIncentive();

		IObservable<string> GetAndObservePromoCode();

		IObservable<bool> GetAndObserveLoadingAddress();

		IObservable<bool> GetAndObserveDropOffSelectionMode();

		IObservable<bool> GetAndObserveOrderCanBeConfirmed();

		IObservable<string> GetAndObserveHashedMarket();

		IObservable<bool> GetAndObserveIsUsingGeo();

		IObservable<List<VehicleType>> GetAndObserveMarketVehicleTypes();

		void SetAddresses(Address pickupAddress, Address destinationAddress);

		void SetDropOffSelectionMode(bool isDropOffSelectionMode);

		IObservable<bool> GetAndObserveIsDestinationModeOpened();

	    IObservable<OrderValidationResult> GetAndObserveOrderValidationResult();

		IObservable<bool> GetAndObserveCanExecuteBookingOperation();

		Task<Tuple<Order, OrderStatusDetail>> GetLastActiveOrder();

        Task POIRefPickupList(string textMatch, int maxRespSize);

        Task POIRefAirLineList(string textMatch, int maxRespSize);

        Guid? GetLastUnratedRide();

		Task<bool> ShouldWarnAboutEstimate();

		Task<bool> ShouldWarnAboutPromoCode();

	    bool ShouldPromptUserToRateLastRide();

		Task<bool> ShouldGoToAccountNumberFlow();

		Task<bool> ValidateAccountNumberAndPrepareQuestions(string accountNumber = null, string customerNumber = null);

		Task<AccountChargeQuestion[]> GetAccountPaymentQuestions();

        bool ValidateAndSaveAccountAnswers(AccountChargeQuestion[] questionsAndAnswers);

        Task<OrderValidationResult> ValidateOrder(CreateOrderRequest order = null);

		void ConfirmValidationOrder ();

        Task Rebook(Order previous);

		Task<Address> GetCurrentAddress();

		Task ResetOrderSettings();

	    bool IsOrderRebooked();

        void CancelRebookOrder();

		Task<bool> ShouldPromptForCvv();
		bool ValidateAndSetCvv(string cvv);

		void DisableBooking();

		Task<bool> UpdateDropOff(Guid orderId);
	}
}

