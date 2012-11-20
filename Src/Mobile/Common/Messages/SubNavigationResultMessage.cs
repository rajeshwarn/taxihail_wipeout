using System;
using TinyMessenger;

namespace apcurium.MK.Booking.Mobile
{
	public class SubNavigationResultMessage<TResult> : TinyMessageBase
	{
		public TResult Result { get; private set; }
		public string MessageId { get; set; }
		
		public SubNavigationResultMessage(object sender, string messageId, TResult result)
			: base(sender)
		{
			Result = result;
			MessageId = messageId;
		}
	}
}

