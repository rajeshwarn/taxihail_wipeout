using System;
using Android.App;
using Android.OS;
using TinyIoC;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Mobile.Client.Helpers;

namespace apcurium.MK.Booking.Mobile.Client
{
	[Activity(Theme = "@android:style/Theme.Dialog")]
	public class SelectItemDialogActivity : Activity
	{
		private string _title;
		private string _ownerId;
		private string[] _items;
		
		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			_title = Intent.GetStringExtra("Title");
			_ownerId = Intent.GetStringExtra("OwnerId");
			_items = Intent.GetStringArrayExtra("Items");
		}
		
		protected override void OnStart()
		{
			base.OnStart();
			ShowDialog();
		}
		
		private void ShowDialog()
		{
			AlertDialogHelper.Show(this, _title, _items, (s,e) => ReturnResult(e.Which));
		}
		
		private void ReturnResult(int itemIndex)
		{
			var messenger = TinyIoCContainer.Current.Resolve<ITinyMessengerHub>();
			messenger.Publish(new SubNavigationResultMessage<int>(this, _ownerId, itemIndex));
			Finish();
		}
	}
}

