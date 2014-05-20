using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using Cirrious.MvvmCross.Binding.BindingContext;
using System.Collections.Generic;
using System.Linq;

namespace apcurium.MK.Booking.Mobile.Client.Views.Order
{
	public partial class InitializeOrderForAccountPaymentView : BaseViewController<InitializeOrderForAccountPaymentViewModel>
	{
		private List<Tuple<int, UILabel, FlatTextField>> _controls;

		public InitializeOrderForAccountPaymentView () : base ("InitializeOrderForAccountPaymentView", null)
		{
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			NavigationController.NavigationBar.Hidden = false;
			NavigationItem.Title = Localize.GetValue("View_InitializeOrderForAccountPayment");
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			View.BackgroundColor = UIColor.FromRGB(242, 242, 242);

			FlatButtonStyle.Green.ApplyTo(btnConfirm);
			btnConfirm.SetTitle(Localize.GetValue("Confirm"), UIControlState.Normal);

			_controls = new List<Tuple<int, UILabel, FlatTextField>> () 
			{
				Tuple.Create (0, lblQuestion1, txtQuestion1),
				Tuple.Create (1, lblQuestion2, txtQuestion2),
				Tuple.Create (2, lblQuestion3, txtQuestion3),
				Tuple.Create (3, lblQuestion4, txtQuestion4),
				Tuple.Create (4, lblQuestion5, txtQuestion5),
				Tuple.Create (5, lblQuestion6, txtQuestion6),
				Tuple.Create (6, lblQuestion7, txtQuestion7),
				Tuple.Create (7, lblQuestion8, txtQuestion8),
			};

			ApplyBinding ();
		}

		private void ApplyBinding()
		{
			var set = this.CreateBindingSet<InitializeOrderForAccountPaymentView, InitializeOrderForAccountPaymentViewModel> ();

			var i = 0;

			if (ViewModel.Questions[0].IsEnabled)
			{
				set.Bind (GetTuple(i).Item2).For (v => v.Text).To (vm => vm.Questions[0].Question);
				set.Bind (GetTuple(i).Item3).For (v => v.Text).To (vm => vm.Questions[0].Answer);
				i++;
			}

			if (ViewModel.Questions[1].IsEnabled)
			{
				set.Bind (GetTuple(i).Item2).For (v => v.Text).To (vm => vm.Questions[1].Question);
				set.Bind (GetTuple(i).Item3).For (v => v.Text).To (vm => vm.Questions[1].Answer);
				i++;
			}

			if (ViewModel.Questions[2].IsEnabled)
			{
				set.Bind (GetTuple(i).Item2).For (v => v.Text).To (vm => vm.Questions[2].Question);
				set.Bind (GetTuple(i).Item3).For (v => v.Text).To (vm => vm.Questions[2].Answer);
				i++;
			}

			if (ViewModel.Questions[3].IsEnabled)
			{
				set.Bind (GetTuple(i).Item2).For (v => v.Text).To (vm => vm.Questions[3].Question);
				set.Bind (GetTuple(i).Item3).For (v => v.Text).To (vm => vm.Questions[3].Answer);
				i++;
			}

			if (ViewModel.Questions[4].IsEnabled)
			{
				set.Bind (GetTuple(i).Item2).For (v => v.Text).To (vm => vm.Questions[4].Question);
				set.Bind (GetTuple(i).Item3).For (v => v.Text).To (vm => vm.Questions[4].Answer);
				i++;
			}

			if (ViewModel.Questions[5].IsEnabled)
			{
				set.Bind (GetTuple(i).Item2).For (v => v.Text).To (vm => vm.Questions[5].Question);
				set.Bind (GetTuple(i).Item3).For (v => v.Text).To (vm => vm.Questions[5].Answer);
				i++;
			}

			if (ViewModel.Questions[6].IsEnabled)
			{
				set.Bind (GetTuple(i).Item2).For (v => v.Text).To (vm => vm.Questions[6].Question);
				set.Bind (GetTuple(i).Item3).For (v => v.Text).To (vm => vm.Questions[6].Answer);
				i++;
			}

			if (ViewModel.Questions[7].IsEnabled)
			{
				set.Bind (GetTuple(i).Item2).For (v => v.Text).To (vm => vm.Questions[7].Question);
				set.Bind (GetTuple(i).Item3).For (v => v.Text).To (vm => vm.Questions[7].Answer);
				i++;
			}

			var nextUnassignedControls = GetTuple (i);
			while (nextUnassignedControls != null)
			{
				// we remove the controls not assigned (which are going to be at the bottom of the view, above the button)
				// the button has multiple constraints of vertical space to the other textfields
				// it also has a constraint to the top of the scrollview, so if the server returns 0 questions, only the button will appear
				nextUnassignedControls.Item2.RemoveFromSuperview ();
				nextUnassignedControls.Item3.RemoveFromSuperview ();
				i++;
				nextUnassignedControls = GetTuple (i);
			}

			set.Bind (btnConfirm).For ("TouchUpInside").To (vm => vm.ConfirmOrder);

			set.Apply ();
		}

		private Tuple<int, UILabel, FlatTextField> GetTuple(int item1Value)
		{
			return _controls.FirstOrDefault (x => x.Item1 == item1Value);
		}
	}
}

