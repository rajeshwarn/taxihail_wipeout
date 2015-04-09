using System;

using Foundation;
using UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.BindingContext;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class ManualPairingForRideLinqView : BaseViewController<ManualPairingForRideLinqViewModel>
	{
		public ManualPairingForRideLinqView()
			: base("ManualPairingForRideLinqView", null)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			FlatButtonStyle.Green.ApplyTo(btnPair);

			var localize = this.Services().Localize;

			NavigationController.NavigationBar.Hidden = false;
			NavigationItem.Title = localize["View_RideLinqPair"];

			lblInstructions.Text = localize["ManualPairingForRideLinQ_Instructions"];

			btnPair.SetTitle(localize["ManualPairingForRideLinQ_Pair"], UIControlState.Normal);

			FlatButtonStyle.Green.ApplyTo(btnPair);

			var bindingSet = this.CreateBindingSet<ManualPairingForRideLinqView, ManualPairingForRideLinqViewModel>();

			bindingSet.Bind(PairingCode1)
				.To(vm => vm.PairingCodeLeft);

			bindingSet.Bind(PairingCode2)
				.To(vm => vm.PairingCodeRight);

			bindingSet.Bind(btnPair)
				.For(v => v.Command)
				.To(vm => vm.PairWithRideLinq);

			bindingSet.Apply();


			PairingCode1.MaxLength = 3;

			PairingCode2.MaxLength = 4;

			PairingCode1.EditingChanged += (object sender, EventArgs e) => 
				{
					if(PairingCode1.Text.Length == 3)
					{
						PairingCode2.BecomeFirstResponder();
					}
				};

			PairingCode2.EditingChanged += (object sender, EventArgs e) => 
				{
					if(PairingCode2.Text.Length == 0)
					{
						PairingCode1.BecomeFirstResponder();
					}
				};
					
		}
	}
}

