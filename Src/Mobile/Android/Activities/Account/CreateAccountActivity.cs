using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.BindingContext;
using CrossUI.Droid;
using CrossUI.Droid.Dialog;
using CrossUI.Droid.Dialog.Elements;
using apcurium.MK.Booking.Mobile.Client.Controls.Dialog;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Account
{
	[Activity(Label = "@string/AccountCreationActivityName", Theme = "@style/LoginTheme", ScreenOrientation = ScreenOrientation.Portrait)]
    public class CreateAccountActivity : BaseBindingActivity<CreateAccountViewModel>
    {
        private const int CellHeightInDip = 42;

        protected override void OnViewModelSet()
		{
			base.OnViewModelSet ();

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
            signMenu.LayoutParameters = new ViewGroup.LayoutParams (ViewGroup.LayoutParams.MatchParent,  ViewGroup.LayoutParams.WrapContent);

            var numberOfFields = ViewModel.HasSocialInfo ? 3 : 5;

            if (ViewModel.Settings.IsPayBackRegistrationFieldRequired.HasValue)
            {
                numberOfFields++;
            }

            signMenu.LayoutParameters.Height = GetDipInPixels(numberOfFields * CellHeightInDip);
            signMenu.Root = InitializeRoot();			
            signMenu.SetScrollContainer (false);

            registerContainer.AddView(signMenu, positionInContainer);
		}

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

			var email = new EntryElement (null, this.Services().Localize["CreateAccountEmailPlaceHolder"], null,  "DialogTop") {IsEmail = true};
			email.Bind(bindings, vm => vm.Data.Email);

			var name = new TaxiHailEntryElement(null, this.Services().Localize["CreateAccountFullNamePlaceHolder"], null, "DialogCenter", InputTypes.TextFlagCapWords);
			name.Bind(bindings, vm => vm.Data.Name);

            var layoutCell = "DialogCenter";
            var phoneEditorCell = "PhoneEditor";
            
            if (ViewModel.HasSocialInfo && !ViewModel.Settings.IsPayBackRegistrationFieldRequired.HasValue)
            {
    	        layoutCell = "DialogBottom";
                phoneEditorCell = "PhoneEditorBottom";
			}

            var phoneNumber = (bindings.BindingContextOwner.BindingContext.DataContext as CreateAccountViewModel).PhoneNumber;
            var phone = new PhoneEditorElement(null, phoneNumber, phoneEditorCell);

            var password = new TaxiHailEntryElement(null, this.Services().Localize["CreateAccountPasswordPlaceHolder"], null, layoutCell) { Password = true };
			password.Bind(bindings, vm => vm.Data.Password);


			var passwordConfirm = new TaxiHailEntryElement(null, this.Services().Localize["CreateAccountPasswordConfirmationPlaceHolder"], null, "DialogBottom") { Password = true };
			passwordConfirm.Bind(bindings, vm => vm.ConfirmPassword);

            var payback = new TaxiHailEntryElement(null, this.Services().Localize["CreateAccountPayBackPlaceHolder"], null, "DialogBottom") { Numeric = true };
            payback.Bind(bindings, vm => vm.Data.PayBack);


            section.Add (new Element[] { email, name, phone });

			if (!ViewModel.HasSocialInfo) {
				section.Add (new Element[] { password, passwordConfirm });
			}

            if (ViewModel.Settings.IsPayBackRegistrationFieldRequired.HasValue)
		    {
                section.Add(new Element[] { payback });
		    }

            root.LayoutName = "fake_rounded";

			root.Add (section);

			return root;
		}
    }
}