using System;
using CoreGraphics;
using Foundation;
using UIKit;
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

            ChangeThemeOfBarStyle();
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

			ApplyBinding();
		}



		private void ApplyBinding()
		{
			var set = this.CreateBindingSet<InitializeOrderForAccountPaymentView, InitializeOrderForAccountPaymentViewModel> ();

			var i = 0;

            for (i = 0; i <= 7; i++)
            {
                if (ViewModel.Questions[i].Model.IsEnabled)
                {
                    set.Bind (GetTuple(i).Item2).For (v => v.Text).To (vm => vm.Questions[i].QuestionLabel);
                    set.Bind (GetTuple(i).Item3).For (v => v.Text).To (vm => vm.Questions[i].Model.Answer);
                    set.Bind (GetTuple(i).Item3).For (v => v.Placeholder).To (vm => vm.Questions[i].QuestionPlaceholder);
                    set.Bind (GetTuple(i).Item3).For (v => v.AccessibilityLabel).To (vm => vm.Questions[i].QuestionPlaceholder);

                    if (i == 0)
                    {
                        set.Bind (GetTuple(i).Item3).For (v => v.MaxLength).To (vm => vm.Questions[i].Model.MaxLength);
                    }
                }
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