using System;
using MonoTouch.Foundation;
using TaxiMobile.Lib.Infrastructure;
using TaxiMobile.Lib.Localization;

namespace TaxiMobile.Localization
{
	public class Resources : IAppResource
	{

		private static string GetValue (string key)
		{
			string localizedValue = NSBundle.MainBundle.LocalizedString (key, "", "");
			return string.IsNullOrEmpty (localizedValue) ? key : localizedValue;
		}

		public AppLanguage CurrentLanguage {
			get { return (AppLanguage)Enum.Parse (typeof(AppLanguage), GetValue ("Language")); }
		}
		
		public string OrderNote{ get { return OrderNoteStatic; } }

		public string OrderNoteGPSApproximate{ get { return OrderNoteGPSApproximateStatic; } }

		public string StatusInvalid{ get { return CarAssignedStatic; } }

		public string CarAssigned{ get { return CarAssignedStatic; } }

		public static string TabBook {
			get { return GetValue ("TabBook"); }
		}

		public static string TabLocations {
			get { return GetValue ("TabLocations"); }
		}

		public static string TabHistory {
			get { return GetValue ("TabHistory"); }
		}

		public static string TabSettings {
			get { return GetValue ("TabSettings"); }
		}

		public static string Close {
			get { return GetValue ("Close"); }
		}

		public static string Now {
			get { return GetValue ("Now"); }
		}

		public static string EstimatePriceOver100 {
			get { return GetValue ("EstimatePriceOver100"); }
		}

		public static string GenericTitle {
			get { return GetValue ("GenericTitle"); }
		}

		public static string NoConnectionTitle {
			get { return GetValue ("NoConnectionTitle"); }
		}

		public static string NoConnectionMessage {
			get { return GetValue ("NoConnectionMessage"); }
		}

		public static string FavoriteLocationsTitle {
			get { return GetValue ("FavoriteLocationsTitle"); }
		}

		public static string LocationHistoryTitle {
			get { return GetValue ("LocationHistoryTitle"); }
		}

		public static string HistoryViewTitle {
			get { return GetValue ("HistoryViewTitle"); }
		}

		public static string LocationViewTitle {
			get { return GetValue ("LocationViewTitle"); }
		}

		public static string LocationDetailViewTitle {
			get { return GetValue ("LocationDetailViewTitle"); }
		}

		public static string HistoryDetailViewTitle {
			get { return GetValue ("HistoryDetailViewTitle"); }
		}

		public static string SettingsViewTitle {
			get { return GetValue ("SettingsViewTitle"); }
		}

		public static string DefaultRideSettingsViewTitle {
			get { return GetValue ("DefaultRideSettingsViewTitle"); }
		}

		public static string SignInViewTitle {
			get { return GetValue ("SignInViewTitle"); }
		}

		public static string EmailLabel {
			get { return GetValue ("EmailLabel"); }
		}

		public static string PasswordLabel {
			get { return GetValue ("PasswordLabel"); }
		}

		public static string SignInButton {
			get { return GetValue ("SignInButton"); }
		}

		public static string SignUpButton {
			get { return GetValue ("SignUpButton"); }
		}

		public static string DontHaveAnAccountLabel {
			get { return GetValue ("DontHaveAnAccountLabel"); }
		}

		public static string LoadingMessage {
			get { return GetValue ("LoadingMessage"); }
		}

		public static string InvalidLoginMessage {
			get { return GetValue ("InvalidLoginMessage"); }
		}

		public static string InvalidLoginMessageTitle {
			get { return GetValue ("InvalidLoginMessageTitle"); }
		}

		public static string BookItButton {
			get { return GetValue ("BookItButton"); }
		}

		public static string BookPickupLabel {
			get { return GetValue ("BookPickupLabel"); }
		}

		public static string PickupTextPlaceholder {
			get { return GetValue ("PickupTextPlaceholder"); }
		}

		public static string AptNumberTextPlaceholder {
			get { return GetValue ("AptNumberTextPlaceholder"); }
		}

		public static string RingCodeTextPlaceholder {
			get { return GetValue ("RingCodeTextPlaceholder"); }
		}

		public static string PickupDateTextPlaceholder {
			get { return GetValue ("PickupDateTextPlaceholder"); }
		}

		public static string PickupTimeTextPlaceholder {
			get { return GetValue ("PickupTimeTextPlaceholder"); }
		}

		public static string BookDestinationLabel {
			get { return GetValue ("BookDestinationLabel"); }
		}

		public static string DestinationTextPlaceholder {
			get { return GetValue ("DestinationTextPlaceholder"); }
		}

		public static string InvalidBookinInfoTitle {
			get { return GetValue ("InvalidBookinInfoTitle"); }
		}

		public static string BookViewInvalidDate {
			get { return GetValue ("BookViewInvalidDate"); }
		}

		public static string InvalidBookinInfo {
			get { return GetValue ("InvalidBookinInfo"); }
		}

		public static string DestinationMapTitle {
			get { return GetValue ("DestinationMapTitle"); }
		}

		public static string PickupMapTitle {
			get { return GetValue ("PickupMapTitle"); }
		}

		public static string ConfirmationViewTitle {
			get { return GetValue ("ConfirmationViewTitle"); }
		}

		public static string CancelBoutton {
			get { return GetValue ("CancelBoutton"); }
		}

		public static string ConfirmButton {
			get { return GetValue ("ConfirmButton"); }
		}

		public static string ConfirmOriginLablel {
			get { return GetValue ("ConfirmOriginLablel"); }
		}

		public static string ConfirmAptRingCodeLabel {
			get { return GetValue ("ConfirmAptRingCodeLabel"); }
		}

		public static string ConfirmDestinationLabel {
			get { return GetValue ("ConfirmDestinationLabel"); }
		}

		public static string ConfirmDateTimeLabel {
			get { return GetValue ("ConfirmDateTimeLabel"); }
		}

		public static string ConfirmNameLabel {
			get { return GetValue ("ConfirmNameLabel"); }
		}

		public static string ConfirmPhoneLabel {
			get { return GetValue ("ConfirmPhoneLabel"); }
		}

		public static string ConfirmPassengersLabel {
			get { return GetValue ("ConfirmPassengersLabel"); }
		}

		public static string ConfirmVehiculeTypeLabel {
			get { return GetValue ("ConfirmVehiculeTypeLabel"); }
		}

		public static string ConfirmNoApt {
			get { return GetValue ("ConfirmNoApt"); }
		}

		public static string ConfirmNoRingCode {
			get { return GetValue ("ConfirmNoRingCode"); }
		}

		public static string ConfirmDestinationNotSpecified {
			get { return GetValue ("ConfirmDestinationNotSpecified"); }
		}

		public static string DateToday {
			get { return GetValue ("DateToday"); }
		}

		public static string TimeNow {
			get { return GetValue ("TimeNow"); }
		}

		public static string StatusViewTitle {
			get { return GetValue ("StatusViewTitle"); }
		}

		public static string StatusDescription {
			get { return GetValue ("StatusDescription"); }
		}

		public static string ChangeBookingSettingsButton {
			get { return GetValue ("ChangeBookingSettingsButton"); }
		}

		public static string CallCompanyButton {
			get { return GetValue ("CallCompanyButton"); }
		}
			
		public static string HistoryInfo {
			get { return GetValue ("HistoryInfo"); }
		}

		public static string NoHistoryLabel {
			get { return GetValue ("NoHistoryLabel"); }
		}

		public static string LocationAddFavorite {
			get { return GetValue ("LocationAddFavorite"); }
		}

		public static string LocationNoHistory {
			get { return GetValue ("LocationNoHistory"); }
		}

		public static string LocationDetailInstructionLabel {
			get { return GetValue ("LocationDetailInstructionLabel"); }
		}

		public static string LocationDetailGiveItANameLabel {
			get { return GetValue ("LocationDetailGiveItANameLabel"); }
		}

		public static string LocationDetailStreetAddressPlaceholder {
			get { return GetValue ("LocationDetailStreetAddressPlaceholder"); }
		}

		public static string LocationDetailAptPlaceholder {
			get { return GetValue ("LocationDetailAptPlaceholder"); }
		}

		public static string LocationDetailRingCodePlaceholder {
			get { return GetValue ("LocationDetailRingCodePlaceholder"); }
		}

		public static string LocationDetailGiveItANamePlaceholder {
			get { return GetValue ("LocationDetailGiveItANamePlaceholder"); }
		}

		public static string SaveButton {
			get { return GetValue ("SaveButton"); }
		}

		public static string DeleteButton {
			get { return GetValue ("DeleteButton"); }
		}

		public static string InvalidAddressTitle {
			get { return GetValue ("InvalidAddressTitle"); }
		}

		public static string InvalidAddressMessage {
			get { return GetValue ("InvalidAddressMessage"); }
		}

		public static string Date {
			get { return GetValue ("Date"); }
		}

		public static string Time {
			get { return GetValue ("Time"); }
		}

		public static string CreateAccountEmail {
			get { return GetValue ("CreateAccountEmail"); }
		}

		public static string CreateAccountTitle {
			get { return GetValue ("CreateAccountTitle"); }
		}

		public static string CreateAccountFirstName {
			get { return GetValue ("CreateAccountFirstName"); }
		}

		public static string CreateAccountLastName {
			get { return GetValue ("CreateAccountLastName"); }
		}

		public static string CreateAccountPhone {
			get { return GetValue ("CreateAccountPhone"); }
		}

		public static string CreateAccountMobile {
			get { return GetValue ("CreateAccountMobile"); }
		}

		public static string CreateAccountLanguage {
			get { return GetValue ("CreateAccountLanguage"); }
		}

		public static string CreateAccountLanguageEn {
			get { return GetValue ("CreateAccountLanguage"); }
		}

		public static string CreateAccountLanguageFr {
			get { return GetValue ("CreateAccountLanguageFr"); }
		}

		public static string CreateAccountPassword {
			get { return GetValue ("CreateAccountPassword"); }
		}

		public static string CreateAccountPasswordConfrimation {
			get { return GetValue ("CreateAccountPasswordConfrimation"); }
		}

		public static string SettingViewLoginInfo {
			get { return GetValue ("SettingViewLoginInfo"); }
		}

		public static string BookItButtonImageName {
			get { return GetValue ("BookItButtonImageName"); }
		}

		public static string DeleteBookItButtonImageNamePressed {
			get { return GetValue ("DeleteBookItButtonImageNamePressed"); }
		}

		public static string InvalidAccountOnActivatedTitle {
			get { return GetValue ("InvalidAccountOnActivatedTitle"); }
		}

		public static string InvalidAccountOnActivatedMessage {
			get { return GetValue ("InvalidAccountOnActivatedMessage"); }
		}

		public static string DoneButton {
			get { return GetValue ("DoneButton"); }
		}

		public static string SignOutButton {
			get { return GetValue ("SignOutButton"); }
		}

		public static string ChangeDefaultRideSettings {
			get { return GetValue ("ChangeDefaultRideSettings"); }
		}

		public static string SendErrorLogButton {
			get { return GetValue ("SendErrorLogButton"); }
		}

		public static string Version {
			get { return GetValue ("Version"); }
		}

		public static string RideSettingsCannotLoadListMessage {
			get { return GetValue ("RideSettingsCannotLoadListMessage"); }
		}

		public static string RideSettingsCannotLoadListTitle {
			get { return GetValue ("RideSettingsCannotLoadListTitle"); }
		}

		public static string RideSettingsName {
			get { return GetValue ("RideSettingsName"); }
		}

		public static string RideSettingsPhone {
			get { return GetValue ("RideSettingsPhone"); }
		}

		public static string RideSettingsPassengers {
			get { return GetValue ("RideSettingsPassengers"); }
		}

		public static string RideSettingsVehiculeType {
			get { return GetValue ("RideSettingsVehiculeType"); }
		}

		public static string RideSettingsCompany {
			get { return GetValue ("RideSettingsCompany"); }
		}

		public static string ConfirmCompanyLabel {
			get { return GetValue ("ConfirmCompanyLabel"); }
		}

		public static string HistoryDetailConfirmationLabel {
			get { return GetValue ("HistoryDetailConfirmationLabel"); }
		}

		public static string HistoryDetailRequestedLabel {
			get { return GetValue ("HistoryDetailRequestedLabel"); }
		}

		public static string HistoryDetailOriginLabel {
			get { return GetValue ("HistoryDetailOriginLabel"); }
		}

		public static string HistoryDetailDestinationLabel {
			get { return GetValue ("HistoryDetailDestinationLabel"); }
		}

		public static string HistoryDetailRebookButton {
			get { return GetValue ("HistoryDetailRebookButton"); }
		}

		public static string HistoryDetailHideButton {
			get { return GetValue ("HistoryDetailHideButton"); }
		}

		public static string HistoryDetailStatusLabel {
			get { return GetValue ("HistoryDetailStatusLabel"); }
		}

		public static string HistoryDetailAptRingCodeLabel {
			get { return GetValue ("HistoryDetailAptRingCodeLabel"); }
		}

		public static string HistoryDetailPickupDateLabel {
			get { return GetValue ("HistoryDetailPickupDateLabel"); }
		}

		public static string StatusStatusLabel {
			get { return GetValue ("StatusStatusLabel"); }
		}

		public static string PickupViewDestinationLabel {
			get { return GetValue ("PickupViewDestinationLabel"); }
		}

		public static string PickupViewPickupLabel {
			get { return GetValue ("PickupViewPickupLabel"); }
		}

		public static string DestinationViewDestinationLabel {
			get { return GetValue ("DestinationViewDestinationLabel"); }
		}

		public static string CallButton {
			get { return GetValue ("CallButton"); }
		}

		public static string TaxiMapTitle {
			get { return GetValue ("TaxiMapTitle"); }
		}

		public static string ErrorCreatingOrderMessage {
			get { return GetValue ("ErrorCreatingOrderMessage"); }
		}

		public static string ErrorCreatingOrderTitle {
			get { return GetValue ("ErrorCreatingOrderTitle"); }
		}

		public static string StatusActionButton {
			get { return GetValue ("StatusActionButton"); }
		}

		public static string StatusActionCancelButton {
			get { return GetValue ("StatusActionCancelButton"); }
		}

		public static string StatusActionBookButton {
			get { return GetValue ("StatusActionBookButton"); }
		}

		public static string StatusConfirmNewBooking {
			get { return GetValue ("StatusConfirmNewBooking"); }
		}

		public static string StatusConfirmCancelRide {
			get { return GetValue ("StatusConfirmCancelRide"); }
		}

		public static string YesButton {
			get { return GetValue ("YesButton"); }
		}

		public static string NoButton {
			get { return GetValue ("NoButton"); }
		}

		public static string StatusConfirmCancelRideError {
			get { return GetValue ("StatusConfirmCancelRideError"); }
		}

		public static string StatusConfirmCancelRideErrorTitle {
			get { return GetValue ("StatusConfirmCancelRideErrorTitle"); }
		}

		public static string HistoryViewStatusButton {
			get { return GetValue ("HistoryViewStatusButton"); }
		}

		public static string LoginForgotPasswordButton {
			get { return GetValue ("LoginForgotPasswordButton"); }
		}

		public static string TechSupportButton {
			get { return GetValue ("TechSupportButton"); }
		}

		public static string PickupOnButtonImageName {
			get { return GetValue ("PickupOnButtonImageName"); }
		}

		public static string PickupOffButtonImageName {
			get { return GetValue ("PickupOffButtonImageName"); }
		}

		public static string DestinationOnButtonImageName {
			get { return GetValue ("DestinationOnButtonImageName"); }
		}

		public static string DestinationOffButtonImageName {
			get { return GetValue ("DestinationOffButtonImageName"); }
		}

		public static string Locating {
			get { return GetValue ("Locating"); }
		}

		public static string OrderNoteStatic {
			get { return GetValue ("OrderNote"); }
		}

		public static string OrderNoteGPS {
			get { return GetValue ("OrderNoteGPS"); }
		}

		public static string OrderNoteGPSApproximateStatic {
			get { return GetValue ("OrderNoteGPSApproximate"); }
		}

		public static string StatusInvalidStatic {
			get { return GetValue ("StatusInvalid"); }
		}

		public static string ParameterButton {
			get { return GetValue ("ParameterButton"); }
		}

		public static string EstimateDistance {
			get { return GetValue ("EstimateDistance"); }
		}

		public static string EstimatePrice {
			get { return GetValue ("EstimatePrice"); }
		}

		public static string NotAvailable {
			get { return GetValue ("NotAvailable"); }
		}

		public static string AboutButton {

			get { return GetValue ("AboutButton"); }
		}

		public static string AboutUsUrl {
			get { return GetValue ("AboutUsUrl"); }
		}

		public static string ResetPasswordTitle {
			get { return GetValue ("ResetPasswordTitle"); }
		}

		public static string ResetPasswordLabel {
			get { return GetValue ("ResetPasswordLabel"); }
		}

		public static string ResetPasswordReset {
			get { return GetValue ("ResetPasswordReset"); }
		}

		public static string ResetPasswordCancel {
			get { return GetValue ("ResetPasswordCancel"); }
		}

		public static string ResetPasswordPlaceholder {
			get { return GetValue ("ResetPasswordPlaceholder"); }
		}

		public static string ResetPasswordInvalidDataTitle {
			get { return GetValue ("ResetPasswordInvalidDataTitle"); }
		}

		public static string ResetPasswordInvalidDataMessage {
			get { return GetValue ("ResetPasswordInvalidDataMessage"); }
		}

		public static string CreateAccoutTitle {
			get { return GetValue ("CreateAccoutTitle"); }
		}

		public static string CreateAccoutEmailLabel {
			get { return GetValue ("CreateAccoutEmailLabel"); }
		}

		public static string CreateAccoutTitleLabel {
			get { return GetValue ("CreateAccoutTitleLabel"); }
		}

		public static string CreateAccoutFirstNameLabel {
			get { return GetValue ("CreateAccoutFirstNameLabel"); }
		}

		public static string CreateAccoutLastNameLabel {
			get { return GetValue ("CreateAccoutLastNameLabel"); }
		}

		public static string CreateAccoutPhoneNumberLabel {
			get { return GetValue ("CreateAccoutPhoneNumberLabel"); }
		}

		public static string CreateAccoutMobileLabel {
			get { return GetValue ("CreateAccoutMobileLabel"); }
		}

		public static string CreateAccoutLanguageLabel {
			get { return GetValue ("CreateAccoutLanguageLabel"); }
		}

		public static string CreateAccoutPasswordLabel {
			get { return GetValue ("CreateAccoutPasswordLabel"); }
		}

		public static string CreateAccoutPasswordConfirmLabel {
			get { return GetValue ("CreateAccoutPasswordConfirmLabel"); }
		}

		public static string CreateAccoutCreate {
			get { return GetValue ("CreateAccoutCreate"); }
		}

		public static string CreateAccoutCancel {
			get { return GetValue ("CreateAccoutCancel"); }
		}

		public static string RideSettingsChargeType {
			get { return GetValue ("RideSettingsChargeType"); }
		}

		public static string CreateAccountInvalidPassword {
			get { return GetValue ("CreateAccountInvalidPassword"); }
		}

		public static string CreateAccountEmptyField {
			get { return GetValue ("CreateAccountEmptyField"); }
		}

		public static string CreateAccountInvalidDataTitle {
			get { return GetValue ("CreateAccountInvalidDataTitle"); }
		}

		public static string CreateAccountTitleMr {
			get { return GetValue ("CreateAccountTitleMr"); }
		}

		public static string CreateAccountTitleMrs {
			get { return GetValue ("CreateAccountTitleMrs"); }
		}

		public static string CreateAccountLanguageFrench {
			get { return GetValue ("CreateAccountLanguageFrench"); }
		}

		public static string CreateAccountLanguageEnglish {
			get { return GetValue ("CreateAccountLanguageEnglish"); }
		}

		public static string CreateAccountErrorTitle {
			get { return GetValue ("CreateAccountErrorTitle"); }
		}

		public static string CreateAccountErrorMessage {
			get { return GetValue ("CreateAccountErrorMessage"); }
		}
		
		public static string CreateAccountErrorNotSpecified {
			get { return GetValue ("CreateAccountErrorNotSpecified"); }
		}
		
		public static string RideSettingsNumberOfTaxi {
			get { return GetValue ("RideSettingsNumberOfTaxi"); }
		}
		
		public static string WarningEstimate {
			get { return GetValue ("WarningEstimate"); }
		}
		
		public static string WarningEstimateTitle {
			get { return GetValue ("WarningEstimateTitle"); }
		}
		
		public static string WarningEstimateDontShow {
			get { return GetValue ("WarningEstimateDontShow"); }
		}
		
		public static string ResetPasswordConfirmationTitle {
			get { return GetValue ("ResetPasswordConfirmationTitle"); }
		}

		public static string ResetPasswordConfirmationMessage {
			get { return GetValue ("ResetPasswordConfirmationMessage"); }
		}
	
		public static string CarAssignedStatic {
			get { return GetValue ("CarAssigned"); }
		}
		
	}
}

