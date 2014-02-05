using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.Client.Style;
using System.Linq;
using apcurium.MK.Common.Extensions;
using System.Drawing;
using apcurium.MK.Booking.Mobile.Client.Helper;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("VehicleTypeAndEstimateView")]
    public class VehicleTypeAndEstimateView : UIView
    {
        private UIView HorizontalDividerTop { get; set; }

        private UIImageView SelectedVehicleType { get; set; }
        private UILabel SelectedVehicleTypeLabel { get; set; }
        private UILabel EstimatedFareLabel { get; set; }

        private UIColor Blue = UIColor.FromRGB(0, 129, 248);

        public VehicleTypeAndEstimateView(IntPtr h):base(h)
        {
            Initialize();
        }
        public VehicleTypeAndEstimateView ( )
        {
            Initialize();
        }

        private void Initialize()
        {
            HorizontalDividerTop = new UIView(new RectangleF(0, 0, Frame.Width, UIHelper.OnePixel));
            HorizontalDividerTop.BackgroundColor = Blue;
            AddSubview(HorizontalDividerTop);

            SelectedVehicleType = new UIImageView(new RectangleF(7f, 4f, 34f, 34f));
            AddSubview(SelectedVehicleType);

            SelectedVehicleTypeLabel = new UILabel();
            SelectedVehicleTypeLabel.Font = UIFont.FromName(FontName.HelveticaNeueBold, 18/2);
            SelectedVehicleTypeLabel.TextColor = Blue;
            SelectedVehicleTypeLabel.ShadowColor = UIColor.Clear;
            AddSubview(SelectedVehicleTypeLabel);

            EstimatedFareLabel = new UILabel();
            EstimatedFareLabel.Lines = 0;
            EstimatedFareLabel.Font = UIFont.FromName(FontName.HelveticaNeueRegular, 28/2);
            EstimatedFareLabel.TextColor = Blue;
            EstimatedFareLabel.ShadowColor = UIColor.Clear;
            AddSubview(EstimatedFareLabel);
        }

        private bool _showEstimate;
        public bool ShowEstimate
        {
            get { return _showEstimate; }
            set
            {
                if (_showEstimate != value)
                {
                    _showEstimate = value;
                    Redraw();
                }
            }
        }

        private string _vehicleType;
        public string VehicleType
        {
            get { return _vehicleType; }
            set
            {
                if (_vehicleType != value)
                {
                    SelectedVehicleType.Image = ImageHelper.ApplyColorToImage(string.Format("{0}_badge.png", value.ToLower()), Blue);

                    SelectedVehicleTypeLabel.Text = value.ToUpper();
                    SelectedVehicleTypeLabel.SizeToFit();
                    SelectedVehicleTypeLabel.SetHorizontalCenter(SelectedVehicleType.Center.X);
                    SelectedVehicleTypeLabel.SetY(SelectedVehicleType.Frame.Bottom);
                }
            }
        }

        public string EstimatedFare
        {
            get { return EstimatedFareLabel.Text; }
            set
            {
                if (EstimatedFareLabel.Text != value)
                {
                    EstimatedFareLabel.Text = value;
                    var sizeThatFits = EstimatedFareLabel.SizeThatFits(new SizeF(Frame.Width - 56f - 5f, Frame.Height - 10f));
                    EstimatedFareLabel.SetWidth(sizeThatFits.Width);
                    EstimatedFareLabel.SetHeight(sizeThatFits.Height);
                    EstimatedFareLabel.SetX(56f);
                    EstimatedFareLabel.SetVerticalCenter(Frame.Height / 2);
                }
            }
        }

        private void Redraw()
        {
            if (ShowEstimate)
            {
                BackgroundColor = UIColor.FromRGB(230, 230, 230).ColorWithAlpha(0.5f);
                HorizontalDividerTop.BackgroundColor = Blue;
                SelectedVehicleType.Hidden = false;
                SelectedVehicleTypeLabel.Hidden = false;
                EstimatedFareLabel.Hidden = false;
            }
            else
            {
                BackgroundColor = UIColor.Clear;
                HorizontalDividerTop.BackgroundColor = UIColor.FromRGB(177, 177, 177);
                SelectedVehicleType.Hidden = true;
                SelectedVehicleTypeLabel.Hidden = true;
                EstimatedFareLabel.Hidden = true;

                // show the buttons to select vehicle type
            }
        }

        private NSLayoutConstraint[] _hiddenContraints { get; set; }
        public override bool Hidden
        {
            get
            {
                return base.Hidden;
            }
            set
            {
                if (base.Hidden != value)
                {
                    base.Hidden = value;
                    new [] { HorizontalDividerTop }.Where(c => c != null).ForEach(c => c.Hidden = value);   
                    if (value)
                    {
                        _hiddenContraints = this.Superview.Constraints != null 
                                            ? this.Superview.Constraints.Where(x => x.FirstItem == this || x.SecondItem == this).ToArray()
                                            : null;
                        if (_hiddenContraints != null)
                        {
                            this.Superview.RemoveConstraints(_hiddenContraints);
                        }
                    }
                    else
                    {
                        if (_hiddenContraints != null)
                        {
                            this.Superview.AddConstraints(_hiddenContraints);
                            _hiddenContraints = null;
                        }
                    }

                    if (Superview != null)
                    {
                        ((OverlayView)Superview).Resize();
                    }
                }
            }
        }
    }
}

