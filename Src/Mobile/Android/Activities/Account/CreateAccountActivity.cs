using Android.App;
using Android.Content.PM;
using Android.Text;
using Android.Widget;
using Android.Views;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.BindingContext;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;
using CrossUI.Droid.Dialog.Elements;
using CrossUI.Droid.Dialog;
using CrossUI.Droid;
using Android.Util;
using Android.Graphics.Drawables;
using Android.Graphics;
using System.Linq;
using apcurium.MK.Booking.Mobile.Client.Helpers;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Account
{
    [Activity(Label = "Create Account", Theme = "@style/LoginTheme", ScreenOrientation = ScreenOrientation.Portrait)]
    public class CreateAccountActivity : BaseBindingActivity<CreateAccountViewModel>
    {

        protected override void OnViewModelSet()
        {
			DroidResources.Initialize (typeof (Resource.Layout));
            SetContentView(Resource.Layout.View_CreateAccount);
            ViewGroup registerContainer = (ViewGroup)FindViewById<LinearLayout>(Resource.Id.registerContainer);
            SetDialog (registerContainer, 0);
            TextView lblTermsAndConditions = (TextView)FindViewById<TextView>(Resource.Id.lblTermsAndConditions);

            var ResourceAcklowledge =  GetString( Resource.String.TermsAndConditionsAcknowledgment);
            var ResourceAcklowledgeBold = GetString(Resource.String.TermsAndConditionsLabel);
            var textWithBold = Html.FromHtml(ResourceAcklowledge.Replace(ResourceAcklowledgeBold, "<b>" + ResourceAcklowledgeBold + "</b>"));
            lblTermsAndConditions.SetText(textWithBold, TextView.BufferType.Spannable);

            var colorTheme = DrawHelper.GetTextColorForBackground(Resources.GetColor(Resource.Color.login_color));
            var checkedIcon = Resources.GetDrawable(Resource.Drawable.@checked_custom);
            var uncheckedIcon = Resources.GetDrawable(Resource.Drawable.@unchecked_custom);                                

            var list = new StateListDrawable();
            list.AddState(new int[]{ Android.Resource.Attribute.StateChecked }, checkedIcon);
            list.AddState(new int[]{ -Android.Resource.Attribute.StateChecked }, uncheckedIcon);
            list.AddState(new int[]{ Android.Resource.Attribute.StatePressed }, checkedIcon);
            list.SetColorFilter(colorTheme, PorterDuff.Mode.Multiply); 
           
            var checkBox = (CheckBox)FindViewById<CheckBox>(Resource.Id.cbTermsAndConditions);
            checkBox.SetButtonDrawable(list);
            checkBox.Invalidate();

            DrawHelper.SupportLoginTextColor(lblTermsAndConditions);
            DrawHelper.SupportLoginTextColor(FindViewById<Button>(Resource.Id.btnCancel));
            DrawHelper.SupportLoginTextColor(FindViewById<Button>(Resource.Id.btnCreate));
            DrawHelper.SupportLoginTextColor(FindViewById<TextView>(Resource.Id.lblTitle));
        }	

        public void SetDialog(ViewGroup registerContainer, int positionInContainer)
		{
            var signMenu = new DialogListView(this);
            signMenu.LayoutParameters = new ViewGroup.LayoutParams (ViewGroup.LayoutParams.FillParent,  ViewGroup.LayoutParams.WrapContent);
            signMenu.LayoutParameters.Height = GetDipInPixels(5 * CellHeightInDip + (ViewModel.HasSocialInfo ? GetDipInPixels(2 * CellHeightInDip) : 0));
            signMenu.Root = InitializeRoot();			
            signMenu.SetScrollContainer (false);

            registerContainer.AddView(signMenu, positionInContainer);
		}

        private int CellHeightInDip = 41;

        private int GetDipInPixels(int value)
        {
            DisplayMetrics dm = new DisplayMetrics();
            WindowManager.DefaultDisplay.GetMetrics(dm);
            return (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, value, dm);
        }

		RootElement InitializeRoot()
		{
			var root = new RootElement();
			var section = new Section ();

			var bindings = this.CreateInlineBindingTarget<CreateAccountViewModel>();

            var email = new EntryElement (null, Localize ("CreateAccountEmailPlaceHolder"), null,  "DialogTop") {IsEmail = true};
			email.Bind(bindings, vm => vm.Data.Email);

            var name = new EntryElement(null, Localize("CreateAccountFullNamePlaceHolder"), null, "DialogCenter");
			name.Bind(bindings, vm => vm.Data.Name);

            var phone = new EntryElement(null, Localize("CreateAccountPhonePlaceHolder"), null, "DialogCenter") { Numeric = true };
			phone.Bind(bindings, vm => vm.Data.Phone);

            var password = new EntryElement(null, Localize("CreateAccountPasswordPlaceHolder"), null, "DialogCenter") { Password = true };
			password.Bind(bindings, vm => vm.Data.Password);

            var passwordConfirm = new EntryElement(null, Localize("CreateAccountPasswordConfirmationPlaceHolder"), null, "DialogBottom") { Password = true };
			passwordConfirm.Bind(bindings, vm => vm.ConfirmPassword);

			section.Add (new Element[] { email, name, phone });

			if (!ViewModel.HasSocialInfo) {
				section.Add (new Element[] { password, passwordConfirm });
			}

            root.LayoutName = "fake_rounded";

			root.Add (section);

			return root;
		}

		

		private string Localize(string value)
		{
			return TinyIoCContainer.Current.Resolve<ILocalization> () [value];
		}
    }
}