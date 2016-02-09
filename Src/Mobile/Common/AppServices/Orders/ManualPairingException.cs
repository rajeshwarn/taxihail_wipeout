using System;
using System.Runtime.Serialization;

namespace apcurium.MK.Booking.Mobile.AppServices.Orders
{
	public class ManualPairingException : Exception, ISerializable
	{
		public int ErrorCode { get; set; }
		
		public ManualPairingException(int errorCode = 0)
			: base("An error occured during manual pairing")
		{
			ErrorCode = errorCode;
		}
	}
}