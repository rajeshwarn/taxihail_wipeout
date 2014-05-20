using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using Cirrious.MvvmCross.Binding.BindingContext;
using System.Collections.Generic;

namespace apcurium.MK.Booking.Mobile.Client.Views.Order
{
	public partial class InitializeOrderForAccountPaymentView : BaseViewController<InitializeOrderForAccountPaymentViewModel>
	{
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

			ApplyBinding ();
			HideDisabledQuestions ();
		}

		private void ApplyBinding()
		{
			var set = this.CreateBindingSet<InitializeOrderForAccountPaymentView, InitializeOrderForAccountPaymentViewModel> ();

			set.Bind (lblQuestion1).For (v => v.Text).To (vm => vm.Questions[0].Question);
			set.Bind (lblQuestion2).For (v => v.Text).To (vm => vm.Questions[1].Question);
			set.Bind (lblQuestion3).For (v => v.Text).To (vm => vm.Questions[2].Question);
			set.Bind (lblQuestion4).For (v => v.Text).To (vm => vm.Questions[3].Question);
			set.Bind (lblQuestion5).For (v => v.Text).To (vm => vm.Questions[4].Question);
			set.Bind (lblQuestion6).For (v => v.Text).To (vm => vm.Questions[5].Question);
			set.Bind (lblQuestion7).For (v => v.Text).To (vm => vm.Questions[6].Question);
			set.Bind (lblQuestion8).For (v => v.Text).To (vm => vm.Questions[7].Question);

			set.Bind (txtQuestion1).For (v => v.Text).To (vm => vm.Questions[0].Answer);
			set.Bind (txtQuestion2).For (v => v.Text).To (vm => vm.Questions[1].Answer);
			set.Bind (txtQuestion3).For (v => v.Text).To (vm => vm.Questions[2].Answer);
			set.Bind (txtQuestion4).For (v => v.Text).To (vm => vm.Questions[3].Answer);
			set.Bind (txtQuestion5).For (v => v.Text).To (vm => vm.Questions[4].Answer);
			set.Bind (txtQuestion6).For (v => v.Text).To (vm => vm.Questions[5].Answer);
			set.Bind (txtQuestion7).For (v => v.Text).To (vm => vm.Questions[6].Answer);
			set.Bind (txtQuestion8).For (v => v.Text).To (vm => vm.Questions[7].Answer);

			set.Bind (btnConfirm).For ("TouchUpInside").To (vm => vm.ConfirmOrder);

			set.Apply ();
		}

		private void HideDisabledQuestions()
		{
//			if (!ViewModel.Questions[0].IsEnabled)
//			{
//				lblQuestion1.RemoveFromSuperview ();
//				txtQuestion1.RemoveFromSuperview ();
//			}
//
//			if (!ViewModel.Questions[1].IsEnabled)
//			{
//				lblQuestion2.RemoveFromSuperview ();
//				txtQuestion2.RemoveFromSuperview ();
//			}
//
//			if (!ViewModel.Questions[2].IsEnabled)
//			{
//				lblQuestion3.RemoveFromSuperview ();
//				txtQuestion3.RemoveFromSuperview ();
//			}
//
//			if (!ViewModel.Questions[3].IsEnabled)
//			{
//				lblQuestion4.RemoveFromSuperview ();
//				txtQuestion4.RemoveFromSuperview ();
//			}
//
//			if (!ViewModel.Questions[4].IsEnabled)
//			{
//				lblQuestion5.RemoveFromSuperview ();
//				txtQuestion5.RemoveFromSuperview ();
//			}
//
//			if (!ViewModel.Questions[5].IsEnabled)
//			{
//				lblQuestion6.RemoveFromSuperview ();
//				txtQuestion6.RemoveFromSuperview ();
//			}
//
//			if (!ViewModel.Questions[6].IsEnabled)
//			{
//				lblQuestion7.RemoveFromSuperview ();
//				txtQuestion7.RemoveFromSuperview ();
//			}
//
//			if (!ViewModel.Questions[7].IsEnabled)
//			{
//				lblQuestion8.RemoveFromSuperview ();
//				txtQuestion8.RemoveFromSuperview ();
//			}
		}
	}
}

