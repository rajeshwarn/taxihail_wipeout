using Android.App;
using Android.Content.PM;
using Android.Widget;
using Android.Views;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.BindingContext;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;
using CrossUI.Droid.Dialog.Elements;
using CrossUI.Droid.Dialog;
using CrossUI.Droid;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Account
{
    [Activity(Label = "Sign Up", ScreenOrientation = ScreenOrientation.Portrait)]
    public class SignUpActivity : BaseBindingActivity<CreateAccountViewModel>
    {
        protected override int ViewTitleResourceId
        {
			get { return Resource.String.View_CreateAccount; }
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
			var signMenu = new DialogListView(this);
			signMenu.LayoutParameters = new ViewGroup.LayoutParams (ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.WrapContent);
			signMenu.Root = InitializeRoot();
			signMenu.SetScrollContainer (false);
			mainContainer.AddView(signMenu, positionInMainContainer);
		}

		RootElement InitializeRoot()
		{
			var root = new RootElement();
			var section = new Section ();

			var email = new EntryElement (Localize ("CreateAccountEmail"), "DialogTop") {IsEmail = true};
		    email.Bind(this, "Value Data.Email");

            var name = new EntryElement(Localize("CreateAccountFullName"), "DialogCenter");
		    name.Bind(this, "Value Data.Name");

            var phone = new EntryElement(Localize("CreateAccountPhone"), "DialogCenter") { Numeric = true };
		    phone.Bind(this, "Value Data.Phone");

            var password = new EntryElement(Localize("CreateAccountPassword"), "DialogCenter") { Password = true };
            password.Bind(this, "Value Data.Password");

            var passwordConfirm = new EntryElement(Localize("CreateAccountPasswordConfrimation"), "DialogBottom") { Password = true };
            passwordConfirm.Bind(this, "Value ConfirmPassword");

			section.Add (new Element[] { email, name, phone });

			if (!ViewModel.HasSocialInfo) {
				section.Add (new Element[] { password, passwordConfirm });
			}

			root.Add (section);

			return root;
		}

		

		private string Localize(string value)
		{
			return TinyIoCContainer.Current.Resolve<ILocalization> () [value];
		}
    }
}