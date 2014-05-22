using System;
using Android.App;
using Android.OS;
using Android.Widget;
using System.Threading.Tasks;
using TinyIoC;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.Client.Messages;
using Android.Views;

namespace apcurium.MK.Booking.Mobile.Client.Activities
{
	[Activity(Theme = "@android:style/Theme.Dialog")]
	public class EditTextDialogActivity : Activity
	{
		protected string _title;
		protected string _message;
		protected string _ownerId;

		protected string _positiveButtonTitle;
		protected string _negativeButtonTitle;

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			_title = Intent.GetStringExtra("Title");
			_message = Intent.GetStringExtra("Message");
			_ownerId = Intent.GetStringExtra("OwnerId");
			_positiveButtonTitle = Intent.GetStringExtra("PositiveButtonTitle");
			_negativeButtonTitle = Intent.GetStringExtra("NegativeButtonTitle");
		}

		protected override void OnStart()
		{
			base.OnStart();
			Display();
		}

		private void Display()
		{
			AlertDialog.Builder alert = new AlertDialog.Builder(this);

			// stops it from disappearing if the user clicks outside
			alert.SetCancelable (false);
			alert.SetTitle(_title);
			alert.SetMessage(_message);

			var input = new EditText(this);
			alert.SetView(input);

			alert.SetPositiveButton(_positiveButtonTitle, (s, e) => SendMessage(input.Text));
			alert.SetNegativeButton(_negativeButtonTitle, (s, e) => SendMessage(null));

			alert.Create();
			alert.Show();
		}

		protected void SendMessage(string buttonTitle)
		{
			Finish();

			Task.Factory.StartNew(() =>
			{
				TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Publish(new ActivityCompleted(this, buttonTitle, _ownerId));
			});
		}
	}
}

