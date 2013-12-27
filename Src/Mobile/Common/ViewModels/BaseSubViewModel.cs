using System;


namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public abstract class BaseSubViewModel<TResult>: BaseViewModel
	{
		public BaseSubViewModel(string messageId)
		{
			MessageId = messageId;
		}

		protected string MessageId {
			get;
			private set;
		}

		protected void Cancel()
		{
			ReturnResult(default(TResult));
		}
		
		protected void ReturnResult(TResult result)
		{
			var message = new SubNavigationResultMessage<TResult>(this, MessageId, result);		
			Close();
            MessengerHub.Publish(message);
		}
	}
}

