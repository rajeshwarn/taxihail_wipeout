using Android.App;
using Android.Content.PM;
using Android.Widget;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Provider;
using Android.Views;
using apcurium.MK.Booking.Mobile.ViewModels;
using CrossUI.Droid;
using CrossUI.Droid.Dialog;
using CrossUI.Droid.Dialog.Elements;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Account
{
    [Activity(Label = "Sign Up",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class SignUpActivity : BaseBindingActivity<CreateAccountViewModel>
    {

        protected override int ViewTitleResourceId
        {
            get { return Resource.String.View_SignUp; }
        }

        protected override void OnViewModelSet()
        {
			DroidResources.Initialize (typeof (Resource.Layout));

			SetContentView(Resource.Layout.View_SignUp);
//			EditText password = FindViewById<EditText>(Resource.Id.SignUpPassword);
//			password.SetTypeface (Android.Graphics.Typeface.Default, Android.Graphics.TypefaceStyle.Normal);
//
//			password = FindViewById<EditText>(Resource.Id.SignUpConfirmPassword);
//			password.SetTypeface (Android.Graphics.Typeface.Default, Android.Graphics.TypefaceStyle.Normal);					
//
			LinearLayout mainContainer = FindViewById<LinearLayout>(Resource.Id.mainContainer);
			DialogListView signMenu = new DialogListView(this);
			signMenu.LayoutParameters = new ViewGroup.LayoutParams (ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.WrapContent);
			mainContainer.AddView(signMenu, 3);
			signMenu.Root = InitializeRoot();
			signMenu.VerticalScrollBarEnabled = false;
			signMenu.SetScrollContainer (false);

        }	

		RootElement InitializeRoot()
		{
			RootElement root = new RootElement("Elements");
			Section section = new Section("Section");

			// The one and only example:
			//https://github.com/slodge/Android.Dialog/blob/master/DialogSampleApp/DialogListViewActivity.cs

			// Make an helper:
			// Create the child with parent, position within, size, other, remove scroll (clean up), layout to use, etc.
			// , section.
			// Try to attach binding

			section = new Section () {
				new EntryElement (null, "Email", null, "EditTextEntry"),
				new EntryElement (null, "Email", null, "EditTextEntry"),
				new EntryElement (null, "Email", null, "EditTextEntry"),
				new EntryElement (null, "Email", null, "EditTextEntry"),
				new EntryElement (null, "Email", null, "EditTextEntry"),
				new EntryElement (null, "Email", null, "EditTextEntry"),
			};

			var Email = new EntryElement () {
				Caption = null, 
				Value = null,
				LayoutName = "EditTextEntry",
				Hint = "Email",
				Password = false
			};

			var Name = new EntryElement () {
				Caption = null, 
				Value = null,
				LayoutName = "EditTextEntry",
				Hint = "Name",
				Password = false
			};

			var Phone = new EntryElement () {
				Caption = null, 
				Value = null,
				LayoutName = "EditTextEntry",
				Hint = "Phone",
				Password = false
			};

			var Password = new EntryElement () {
				Caption = null, 
				Value = null,
				LayoutName = "EditTextEntry",
				Hint = "Password",
				Password = true
			};

			var PasswordConfirm = new EntryElement () {
				Caption = null, 
				Value = null,
				LayoutName = "EditTextEntry",
				Hint = "Confirm Password",
				Password = true
			};

			root.Add(section);

			return root;
		}
    }
}