using System;
using System.Threading;
using System.Threading.Tasks;
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
		Task<bool> ValidatePromotionUseConditions();

		Task SetAddress(Address address);
		Task SetPickupAptAndRingCode(string apt, string ringCode);
		Task<Address> SetAddressToUserLocation(CancellationToken cancellationToken = default(CancellationToken));
		Task ClearDestinationAddress();

        Task SetAddressToCoordinate(Position coordinate, CancellationToken cancellationToken);

		Task SetPickupDate(DateTime? date);

		Task ToggleBetweenPickupAndDestinationSelectionMode();
		Task ToggleIsDestinationModeOpened(bool? forceValue = null);

		Task ValidatePickupTime();
		Task ValidatePickupAndDestination();
		Task<Tuple<Order, OrderStatusDetail>> ConfirmOrder();

		Task SetVehicleType (int? vehicleTypeId);
		Task SetBookingSettings(BookingSettings bookingSettings);
		Task SetAccountNumber (string accountNumber);
		void SetNoteToDriver(string text);
		void SetPromoCode(string code);

		IObservable<Address> GetAndObservePickupAddress();
		IObservable<Address> GetAndObserveDestinationAddress();
		IObservable<AddressSelectionMode> GetAndObserveAddressSelectionMode();
		IObservable<int?> GetAndObserveVehicleType();
		IObservable<BookingSettings> GetAndObserveBookingSettings();
		IObservable<DateTime?> GetAndObservePickupDate();
		IObservable<string> GetAndObserveEstimatedFare();
		IObservable<string> GetAndObserveNoteToDriver();
		IObservable<string> GetAndObservePromoCode();
		IObservable<bool> GetAndObserveLoadingAddress();
		IObservable<bool> GetAndObserveOrderCanBeConfirmed();
		IObservable<string> GetAndObserveMarket();
		IObservable<bool> GetAndObserveIsDestinationModeOpened();

		Task<Tuple<Order, OrderStatusDetail>> GetLastActiveOrder();

        Guid? GetLastUnratedRide();

		Task<bool> ShouldWarnAboutEstimate();
		Task<bool> ShouldWarnAboutPromoCode();

	    bool ShouldPromptUserToRateLastRide();

		Task<bool> ShouldGoToAccountNumberFlow();
		Task<bool> ValidateAccountNumberAndPrepareQuestions(string accountNumber = null);
		Task<AccountChargeQuestion[]> GetAccountPaymentQuestions();
        bool ValidateAndSaveAccountAnswers(AccountChargeQuestion[] questionsAndAnswers);

		Task<OrderValidationResult> ValidateOrder();
		void ConfirmValidationOrder ();

        Task Rebook(Order previous);

		Task<Address> GetCurrentAddress();

		Task ResetOrderSettings();

	    bool IsOrderRebooked();

        void CancelRebookOrder();
    }
}

