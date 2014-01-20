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
	[Activity(Label = "Sign Up", Theme = "@style/LoginTheme", ScreenOrientation = ScreenOrientation.Portrait)]
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

			var bindings = this.CreateInlineBindingTarget<CreateAccountViewModel>();

			var email = new EntryElement (null, Localize ("CreateAccountEmail"), null,  "DialogTop") {IsEmail = true};
			email.Bind(bindings, vm => vm.Data.Email);

            var name = new EntryElement(null, Localize("CreateAccountFullName"), null, "DialogCenter");
			name.Bind(bindings, vm => vm.Data.Name);

            var phone = new EntryElement(null, Localize("CreateAccountPhone"), null, "DialogCenter") { Numeric = true };
			phone.Bind(bindings, vm => vm.Data.Phone);

            var password = new EntryElement(null, Localize("CreateAccountPassword"), null, "DialogCenter") { Password = true };
			password.Bind(bindings, vm => vm.Data.Password);

            var passwordConfirm = new EntryElement(null, Localize("CreateAccountPasswordConfrimation"), null, "DialogBottom") { Password = true };
			passwordConfirm.Bind(bindings, vm => vm.ConfirmPassword);

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