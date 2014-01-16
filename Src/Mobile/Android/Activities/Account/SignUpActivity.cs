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
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;
using System;


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

			ViewGroup mainContainer = (ViewGroup)FindViewById<LinearLayout>(Resource.Id.mainContainer);

			SetDialog (mainContainer, 3);
        }	

		public void SetDialog(ViewGroup mainContainer, int positionInMainContainer)
		{
			DialogListView signMenu = new DialogListView(this);
			signMenu.LayoutParameters = new ViewGroup.LayoutParams (ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.WrapContent);
			signMenu.Root = InitializeRoot(signMenu);
			signMenu.SetScrollContainer (false);
			mainContainer.AddView(signMenu, positionInMainContainer);
//			SetPasswordTypeface (mainContainer);

		}

//		public void SetPasswordTypeface(ViewGroup container)
//		{
//			for (int i=0; i<((ViewGroup)container).ChildCount; ++i) {
//				View nextChild = ((ViewGroup)container).GetChildAt (i);
//				if (typeof(ViewGroup).IsInstanceOfType (nextChild)) {
//					SetPasswordTypeface ((ViewGroup)nextChild);
//				} else {
//					// Check if it's an EditText with attribute for password
//					if (typeof(EditText).IsInstanceOfType (nextChild)) { // its a password
//						((EditText)nextChild).SetTypeface (Android.Graphics.Typeface.Default, Android.Graphics.TypefaceStyle.Normal);
//					}
//				}
//			}
//		}

		RootElement InitializeRoot(DialogListView dlv)
		{
			RootElement root = new RootElement();

			Section section = new Section ();

			section = new Section ();

			var Email = new TaxiHailEntryElement (Localize ("CreateAccountEmail"), "DialogTop",  
			                                      (s, e) => { 
				ViewModel.Data.Email = ((EntryElement)s).Value; 
			});

			var Name = new TaxiHailEntryElement (Localize ("CreateAccountFullName"), "DialogCenter",  
			                                     (s, e) => { 
				ViewModel.Data.Name = ((EntryElement)s).Value; 
			});

			var Phone = new TaxiHailEntryElement (Localize ("CreateAccountPhone"), "DialogCenter",  
			                                      (s, e) => { 
				ViewModel.Data.Phone = ((EntryElement)s).Value; 
			});

			var Password = new TaxiHailEntryElement (Localize ("CreateAccountPassword"), "DialogCenter",  
			                                         (s, e) => { 
				ViewModel.Data.Password = ((EntryElement)s).Value; 
			}, true);

			var PasswordConfirm = new TaxiHailEntryElement (Localize ("CreateAccountPasswordConfrimation"), "DialogBottom",  
			                                                (s, e) => {
				ViewModel.ConfirmPassword = ((EntryElement)s).Value;
			}, true);

			section.Add (new Element[] { Email, Name, Phone });

			if (!ViewModel.HasSocialInfo) {
				section.Add (new Element[] { Password, PasswordConfirm });
			}

			root.Add (section);

			return root;
		}

		public class TaxiHailEntryElement: EntryElement
		{
			public TaxiHailEntryElement(string hint, string layoutName, EventHandler action, bool isPassword = false, string prePopulatedWith = null):base(null,hint,prePopulatedWith, layoutName)
			{
				Password = isPassword;
				ValueChanged += action;
			}
		}

		private string Localize(string value)
		{
			return TinyIoCContainer.Current.Resolve<ILocalization> () [value];
		}
    }
}