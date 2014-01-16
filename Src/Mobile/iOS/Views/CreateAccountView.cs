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

			var section = new Section () {
				new TaxiHailEntryElement (string.Empty, Localize.GetValue ("CreateAccountEmail"), ViewModel.Data.Email).Bind(this, "Value Data.Email"),
				new TaxiHailEntryElement (string.Empty, Localize.GetValue ("CreateAccountFullName"), ViewModel.Data.Name).Bind(this, "Value Data.Name"),
				new TaxiHailEntryElement (string.Empty, Localize.GetValue ("CreateAccountPhone"), ViewModel.Data.Phone).Bind(this, "Value Data.Phone")
			};

			if (!ViewModel.HasSocialInfo) {
				constraintTableViewHeight.Constant += 40*2;
				section.AddAll (new List<Element> { 
					new TaxiHailEntryElement (string.Empty, Localize.GetValue ("CreateAccountPassword"), ViewModel.Data.Password, true).Bind(this, "Value Data.Password"), 
					new TaxiHailEntryElement (string.Empty, Localize.GetValue ("CreateAccountPasswordConfrimation"), ViewModel.ConfirmPassword, true).Bind(this, "Value ConfirmPassword")
				});
			}

			var root = new RootElement(){ section };

			var dialogView = new TaxiHailDialogViewController (UITableViewStyle.Plain, root, true).View;
			dialogView.Frame = new RectangleF(0, 0, tableView.Frame.Width, tableView.Frame.Height);
			tableView.BackgroundColor = UIColor.Clear;
			tableView.AddSubview(dialogView);

			var set = this.CreateBindingSet<CreateAccountView, CreateAccountViewModel>();

			set.Bind(btnCancel)
				.For("TouchUpInside")
				.To(vm => vm.CloseCommand);

			set.Bind(btnCreate)
				.For("TouchUpInside")
				.To(vm => vm.CreateAccount);

			set.Apply ();
        }
    }
}

