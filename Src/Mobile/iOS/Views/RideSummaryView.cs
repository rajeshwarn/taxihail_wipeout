using System;
using Cirrious.MvvmCross.Binding.BindingContext;
using UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Localization;
using Cirrious.MvvmCross.Binding.Touch.Views;
using apcurium.MK.Booking.Mobile.Client.Order;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using System.Linq;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class RideSummaryView  : BaseViewController<RideSummaryViewModel>
	{     
        private MvxActionBasedTableViewSource _source;

		public RideSummaryView() : base("RideSummaryView", null)
		{
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

            NavigationController.NavigationBar.Hidden = false;
            NavigationItem.HidesBackButton = true;
            NavigationItem.Title = Localize.GetValue("RideSummaryTitleText");
			gratuityLabel.Text = Localize.GetValue("BookingGratuityText");
            ChangeThemeOfBarStyle();
			FlatButtonStyle.Green.ApplyTo (btnGratuity0);
			FlatButtonStyle.Green.ApplyTo (btnGratuity1);
			FlatButtonStyle.Green.ApplyTo (btnGratuity2);
			FlatButtonStyle.Green.ApplyTo (btnGratuity3);
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

            View.BackgroundColor = UIColor.FromRGB(242, 242, 242);

            lblSubTitle.Text = String.Format(Localize.GetValue ("RideSummarySubTitleText"), this.Services().Settings.TaxiHail.ApplicationName);

            PrepareTableView();

			_hasSelectedGratuity = !ViewModel.NeedToSelectGratuity;

            InitializeBindings();
		}

        public override void ViewWillDisappear(bool animated)
        {
            if (IsMovingFromParentViewController)
            {
                // Back button pressed
                ViewModel.PrepareNewOrder.ExecuteIfPossible();
            }

            base.ViewWillDisappear(animated);
        }

        private void PrepareTableView()
        {
            _source = new MvxActionBasedTableViewSource(
                tableRatingList,
                UITableViewCellStyle.Default,
                BookRatingCell.Identifier,
                BookRatingCell.BindingText,
                UITableViewCellAccessory.None);

            _source.CellCreator = (tableView, indexPath, item) =>
            {
                var cell = BookRatingCell.LoadFromNib(tableView);
                cell.RemoveDelay();
                return cell;
            };

            tableRatingList.Source = _source;

			SetGratuityButtons();
        }

        private void InitializeBindings()
        {
            ViewModel.PropertyChanged += (sender, e) =>
            {
                if(e.PropertyName == "RatingList")
                {
                    ResizeTableView();
                }
				ShowDoneButton();
            };

            var set = this.CreateBindingSet<RideSummaryView, RideSummaryViewModel> ();

            set.Bind(_source)
                .For(v => v.ItemsSource)
                .To(vm => vm.RatingList);

			set.Bind (tableRatingList)
                .For (v => v.Hidden)
                .To (vm => vm.NeedToSelectGratuity);

			set.Bind (gratuityView)
                .For (v => v.Hidden)
                .To (vm => vm.NeedToSelectGratuity)
                .WithConversion ("BoolInverter");

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
			var button = ((FlatButton)sender);
			button.Selected = true;
			ViewModel.SelectGratuity.Execute((long)button.Tag);
			ShowDoneButton();
		}

		private FlatButton[] _gratuityButtons = null;

		private void UnselectButtons()
		{
			for (var i = 0; i < 4; i++)
			{
				_gratuityButtons[i].Selected = false;
			}
		}

		private bool _hasSelectedGratuity = true;
		private void ShowDoneButton ()
		{
			_hasSelectedGratuity = !ViewModel.NeedToSelectGratuity || ViewModel.GratuitySelectionStates.Any(x => x);

			var buttonTextResource = _hasSelectedGratuity && ViewModel.NeedToSelectGratuity
				? "Next" 
				: "Done";

			var buttonHidden = !ViewModel.CanRate || !_hasSelectedGratuity;

			NavigationItem.RightBarButtonItem = buttonHidden ? null : new UIBarButtonItem (Localize.GetValue (buttonTextResource), UIBarButtonItemStyle.Bordered, async (o, e) => {
				if (ViewModel.CanRate)
				{
					NavigationRightBarButtonClicked ();
				}
			});
		}

		private void NavigationRightBarButtonClicked ()
		{
			if (ViewModel.NeedToSelectGratuity)
			{
				ViewModel.PayGratuity.Execute(null);
			}
			else
			{
				ViewModel.RateOrderAndNavigateToHome.ExecuteIfPossible(null);
			}
		}

        private void ResizeTableView()
        {
            if (ViewModel.RatingList != null)
            {
                constraintRatingTableHeight.Constant = BookRatingCell.Height * ViewModel.RatingList.Count;
            }
            else
            {
                constraintRatingTableHeight.Constant = 0;
            }
        }
	}
}

