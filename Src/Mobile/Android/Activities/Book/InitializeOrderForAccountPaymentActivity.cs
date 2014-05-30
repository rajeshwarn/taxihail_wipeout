using System;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using Android.App;
using Android.Content.PM;
using Android.Widget;
using System.Linq;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
	[Activity(Label = "InitializeOrderForAccountPaymentActivity", Theme = "@style/MainTheme", ScreenOrientation = ScreenOrientation.Portrait)]
	public class InitializeOrderForAccountPaymentActivity : BaseBindingActivity<InitializeOrderForAccountPaymentViewModel>
	{
		protected override void OnViewModelSet()
		{
			base.OnViewModelSet ();
			SetContentView(Resource.Layout.View_InitializeOrderForAccountPayment);

			//correct here because the viewmodel is simple, only one property and fetch once
			ViewModel.PropertyChanged += (sender, e) =>
			{
				if (ViewModel.Questions != null
					&& ViewModel.Questions.Any())
				{
					for (int i = 0; i < ViewModel.Questions.Count; i++) {

						if(ViewModel.Questions[i].Model.IsEnabled
							&& ViewModel.Questions[i].Model.MaxLength.HasValue)
						{
							var resID = BaseContext.Resources.GetIdentifier("editQuestion" + i, "id", BaseContext.PackageName);
							var editText = FindViewById<EditText>(resID);
							editText.SetFilters(new [] {new Android.Text.InputFilterLengthFilter(ViewModel.Questions[i].Model.MaxLength.Value)});
						}
					}
				}
			};
		}
	}
}

