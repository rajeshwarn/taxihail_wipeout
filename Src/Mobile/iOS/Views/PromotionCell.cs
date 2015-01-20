using System;
using UIKit;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using apcurium.MK.Booking.Mobile.Client.Localization;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    public partial class PromotionCell : MvxTableViewCell
    {
        public static nfloat Height = 44f;

        public PromotionCell(IntPtr handle) : base(handle)
        {
            SelectionStyle = UITableViewCellSelectionStyle.None;
        }       

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            var set = this.CreateBindingSet<PromotionCell, PromotionItemViewModel>();

            set.Bind(lblName).For(v => v.Text).To(vm => vm.Name);
            set.Bind(lblExpires).For(v => v.Text).To(vm => vm.ExpiringSoonWarning);
            set.Bind(lblDescription).For(v => v.Text).To(vm => vm.Description);
			set.Bind (lblProgress).For (v => v.Text).To (vm => vm.ProgressDescription);
            set.Bind(btnApplyPromo).For("TouchUpInside").To(vm => vm.SelectedCommand);

            set.Apply(); 

            ApplyStyle();
        }

        private bool _hideBottomBar;
        public bool HideBottomBar
        {
            get { return _hideBottomBar; }
            set
            { 
                ((CustomCellBackgroundView)BackgroundView).HideBottomBar = value;
                _hideBottomBar = value;
            }
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    lblDescription.Hidden = !_isExpanded;
					lblProgress.Hidden = !_isExpanded;
                    btnApplyPromo.Hidden = !_isExpanded;
                    imgExpanded.Hidden = !_isExpanded;
                    imgCollapsed.Hidden = _isExpanded;
                }
            }
        }

        private void ApplyStyle()
        {
            lblDescription.Hidden = true;
			lblProgress.Hidden = true;
            btnApplyPromo.Hidden = true;
            imgExpanded.Hidden = true;

            BackgroundView = new CustomCellBackgroundView(this.ContentView.Frame, 10, UIColor.White, UIColor.Clear) 
            {
                HideBottomBar = HideBottomBar
            };

            lblName.TextColor = UIColor.FromRGB(44, 44, 44);
            lblName.Font = UIFont.FromName(FontName.HelveticaNeueBold, 28/2);

            lblExpires.TextColor = UIColor.FromRGB(251, 0, 10);
            lblExpires.Font = UIFont.FromName(FontName.HelveticaNeueMedium, 22/2);

            FlatButtonStyle.Green.ApplyTo(btnApplyPromo);
            btnApplyPromo.Font = UIFont.FromName(FontName.HelveticaNeueRegular, 28 / 2);
            btnApplyPromo.SetTitle(Localize.GetValue("PromoBookRide"), UIControlState.Normal);

            ContentView.BackgroundColor = UIColor.Clear;
            BackgroundColor = UIColor.Clear;
        }
    }
}

