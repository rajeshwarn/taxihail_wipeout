using System.Linq;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.Touch.Views;
using UIKit;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.Client.Views;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using CoreGraphics;
using System;
using System.Collections.Generic;

namespace apcurium.MK.Booking.Mobile.Client.Order
{
	public partial class BookRatingView : BaseViewController<BookRatingViewModel>
	{
        public BookRatingView() : base("BookRatingView", null)
		{
		}
		
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			NavigationController.NavigationBar.Hidden = false;
			NavigationItem.Title = Localize.GetValue("View_BookRating");
			gratuityLabel.Text = Localize.GetValue("BookingGratuityText");
			ChangeThemeOfBarStyle();
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			if (ViewModel.CanUserLeaveScreen ())
			{
				NavigationItem.HidesBackButton = false;
			} else
			{
				NavigationItem.HidesBackButton = true;
			}

			View.BackgroundColor = UIColor.FromRGB (242, 242, 242);
			ratingTableView.BackgroundColor = UIColor.Clear;
			ratingTableView.SeparatorColor = UIColor.Clear;
			ratingTableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			ratingTableView.DelaysContentTouches = false;

			var btnSubmit = new FlatButton (new CGRect (8f, BottomPadding, 304f, 41f));
			FlatButtonStyle.Green.ApplyTo (btnSubmit);
			FlatButtonStyle.Green.ApplyTo (btnGratuity0);
			FlatButtonStyle.Green.ApplyTo (btnGratuity1);
			FlatButtonStyle.Green.ApplyTo (btnGratuity2);
			FlatButtonStyle.Green.ApplyTo (btnGratuity3);

			btnSubmit.SetTitle (Localize.GetValue ("Submit"), UIControlState.Normal);

			ViewModel.PropertyChanged += async (sender, e) => {
				if (e.PropertyName == "CanRate")
				{
					ShowDoneButton ();
				}
			};

			var source = new MvxActionBasedTableViewSource (
				             ratingTableView,
				             UITableViewCellStyle.Default,
				             BookRatingCell.Identifier,
				             BookRatingCell.BindingText,
				             UITableViewCellAccessory.None);
			source.CellCreator = (tableView, indexPath, item) => {
				var cell = BookRatingCell.LoadFromNib (tableView);
				cell.RemoveDelay ();
				return cell;
			};

			ratingTableView.Source = source;

			SetGratuityButtons();

			var set = this.CreateBindingSet<BookRatingView, BookRatingViewModel> ();

			set.Bind (ratingTableView)
                .For (v => v.Hidden)
                .To (vm => vm.NeedToSelectGratuity);

			set.Bind (gratuityView)
                .For (v => v.Hidden)
                .To (vm => vm.NeedToSelectGratuity)
                .WithConversion ("BoolInverter");

			set.Bind (btnSubmit)
                .For ("TouchUpInside")
                .To (vm => vm.RateOrder);

			set.Bind (btnSubmit)
                .For (v => v.Hidden)
                .To (vm => vm.CanRate)
                .WithConversion ("BoolInverter");

			set.Bind (source)
                .For (v => v.ItemsSource)
                .To (vm => vm.RatingList);

			set.Apply ();
		}

		private void SetGratuityButtons ()
		{
			if (_gratuityButtons == null)
			{
				_gratuityButtons = new FlatButton[] { btnGratuity0, btnGratuity1, btnGratuity2, btnGratuity3 };
			}

			for (var i = 0; i < 4; i++)
			{
				_gratuityButtons[i].Tag = i;
				_gratuityButtons[i].TouchUpInside += GratuityClicked;
			}
		}

		private void GratuityClicked(object sender, EventArgs e)
		{
			UnselectButtons();
			((FlatButton)sender).Selected = true;
		}

		private FlatButton[] _gratuityButtons = null;

		private void UnselectButtons()
		{
			for (var i = 0; i < 4; i++)
			{
				_gratuityButtons[i].Selected = false;
			}
		}

		private void ShowDoneButton()
		{
			var buttonTextResource = ViewModel.NeedToSelectGratuity ? "Next" : "Done";

			NavigationItem.RightBarButtonItem = !ViewModel.CanRate ? null : new UIBarButtonItem (Localize.GetValue (buttonTextResource), UIBarButtonItemStyle.Bordered, async (o, e) => {
				if (ViewModel.CanRate) 
				{
					NavigationRightBarButtonClicked();
				}
			});
		}

		private void NavigationRightBarButtonClicked ()
		{
			if (ViewModel.NeedToSelectGratuity)
			{
				ViewModel.PayGratuity.Execute(null);
				ShowDoneButton();
			}
			else
			{
				ViewModel.RateOrder.Execute(null);
			}
		}
	}
}

