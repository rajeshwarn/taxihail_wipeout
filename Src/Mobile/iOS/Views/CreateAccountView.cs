using System.Collections.Generic;
using System.Drawing;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Views;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.ViewModels;
using CrossUI.Touch.Dialog.Elements;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using CrossUI.Touch.Dialog;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class CreateAccountView : BaseViewController<CreateAccountViewModel>
    {
        public CreateAccountView() : base("CreateAccountView", null)
        {
        }

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			NavigationController.NavigationBar.Hidden = true;
		}

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

			View.BackgroundColor = UIColor.FromRGB(0, 72, 129);

			FlatButtonStyle.Main.ApplyTo (btnCreate);

			BuildTableView (tableView);

			var set = this.CreateBindingSet<CreateAccountView, CreateAccountViewModel>();

			set.Bind(btnCancel)
				.For("TouchUpInside")
				.To(vm => vm.CloseCommand);

			set.Bind(btnCreate)
				.For("TouchUpInside")
				.To(vm => vm.CreateAccount);

			set.Apply ();
        }

		private void BuildTableView(UIView container)
		{
			var section = new Section () {
				new TaxiHailEntryElement (string.Empty, Localize.GetValue ("CreateAccountEmailPlaceHolder"), ViewModel.Data.Email)
				{
					KeyboardType = UIKeyboardType.EmailAddress		
				}.Bind(this, "Value Data.Email"),
				new TaxiHailEntryElement (string.Empty, Localize.GetValue ("CreateAccountFullNamePlaceHolder"), ViewModel.Data.Name).Bind(this, "Value Data.Name"),
				new TaxiHailEntryElement (string.Empty, Localize.GetValue ("CreateAccountPhonePlaceHolder"), ViewModel.Data.Phone){
					KeyboardType = UIKeyboardType.PhonePad		
				}.Bind(this, "Value Data.Phone")
			};

			if (!ViewModel.HasSocialInfo) {
				constraintTableViewHeight.Constant += 40*2;
				section.AddAll (new List<Element> { 
					new TaxiHailEntryElement (string.Empty, Localize.GetValue ("CreateAccountPasswordPlaceHolder"), ViewModel.Data.Password, true).Bind(this, "Value Data.Password"), 
					new TaxiHailEntryElement (string.Empty, Localize.GetValue ("CreateAccountPasswordConfirmationPlaceHolder"), ViewModel.ConfirmPassword, true).Bind(this, "Value ConfirmPassword")
				});
			}

			var root = new RootElement(){ section };

			tableView.BackgroundColor = UIColor.Clear;
			tableView.AddSubview(new TaxiHailDialogViewController (UITableViewStyle.Plain, root, true).TableView);
		}
    }
}

