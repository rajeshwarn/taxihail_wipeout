namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public partial class Str
	{
		public static string ErrorCreatingOrderTitle {
			get{				
				return _r.GetString ("ErrorCreatingOrderTitle");
			}
		}
		public static string NoCreditCardSelectedMessage {
			get{				
				return _r.GetString ("NoCreditCardSelected");
			}
		}

		public static string NoAmountSelectedMessage {
			get{				
				return _r.GetString ("NoAmountSelectedMessage");
			}
		}

		public  static string NoOrderId {
			get{				
				return _r.GetString ("NoOrderId");
			}
		}

		public static string CmtTransactionErrorMessage {
			get{				
				return _r.GetString ("CmtTransactionErrorMessage");
			}
		}
		public static string SendMessageErrorMessage {
			get{				
				return _r.GetString ("SendMessageErrorMessage");
			}
		}

		public static string CmtTransactionSuccessMessage {
			get{				
				return _r.GetString ("CmtTransactionSuccessMessage");
			}
		}
		public static string CmtTransactionSuccessTitle {
			get{				
				return _r.GetString ("CmtTransactionSuccessTitle");
			}
		}
		public static string CmtTransactionResendConfirmationButtonText {
			get{				
				return _r.GetString ("CmtTransactionResendConfirmationButtonText");
			}
		}
		
		public static string TaxiServerDownMessage {
			get{				
				return _r.GetString ("TaxiServerDownMessage");
			}
		}


		public static string GetPaymentConfirmationMessageToDriver(string formattedAmount)
		{
			return  string.Format (_r.GetString ("PaymentConfirmationMessageToDriver"), formattedAmount);		
		}



		public static string GetServerErrorCreatingOrder(int phoneNumberId)
		{
            return  string.Format (_r.GetString ("ServiceError_ErrorCreatingOrderMessage"), _appSettings.ApplicationName, _config.GetSetting( "DefaultPhoneNumberDisplay" ));
			
		}

	

	}
    
}
