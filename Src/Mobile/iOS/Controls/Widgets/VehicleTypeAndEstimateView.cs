using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.Client.Style;
using System.Linq;
using apcurium.MK.Common.Extensions;
using System.Drawing;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using apcurium.MK.Booking.Api.Contract.Resources;
using System.Collections.Generic;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("VehicleTypeAndEstimateView")]
    public class VehicleTypeAndEstimateView : UIView
    {
        private UIView HorizontalDividerTop { get; set; }
        private VehicleTypeView EstimateSelectedVehicleType { get; set; }
		private VehicleTypeView EtaBadge { get; set; }
        private UILabel EstimatedFareLabel { get; set; }
		private UILabel EtaLabel { get; set; }
        private UIView VehicleSelection { get; set; }
		private UIView EtaContainer { get; set; }

		private const float VehicleSelectionHeight = 52f;
		private const float EtaContainerHeight = 23f;
		private const float VehicleLeftBadgeWidth = 56f;
		private const float LabelPadding = 5f;

        public Action<VehicleType> VehicleSelected { get; set; }

		private NSLayoutConstraint _heightConstraint;

        public VehicleTypeAndEstimateView(IntPtr h) : base(h)
        {
            Initialize();
        }

        public VehicleTypeAndEstimateView ()
        {
            Initialize();
        }

        private void Initialize()
        {
			_heightConstraint = NSLayoutConstraint.Create(this, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1.0f, VehicleSelectionHeight);
			AddConstraint(_heightConstraint);

            HorizontalDividerTop = Line.CreateHorizontal(Frame.Width, Theme.LabelTextColor);

            EstimateSelectedVehicleType = new VehicleTypeView(new RectangleF(0f, 0f, 50f, this.Frame.Height));

            VehicleSelection = new UIView (this.Bounds);

            EstimatedFareLabel = new UILabel
            {
				AdjustsFontSizeToFitWidth = true,
                BackgroundColor = UIColor.Clear,
                Lines = 1,
				Font = UIFont.FromName(FontName.HelveticaNeueLight, 32 / 2),
                TextAlignment = NaturalLanguageHelper.GetTextAlignment(),
                TextColor = Theme.LabelTextColor,
                ShadowColor = UIColor.Clear
            };

			EstimatedFareLabel.SetWidth(Frame.Width - VehicleLeftBadgeWidth - LabelPadding);
			EstimatedFareLabel.SetHeight(Frame.Height / 2);
			EstimatedFareLabel.SetHorizontalCenter((Frame.Width / 2) + (VehicleLeftBadgeWidth / 2) - LabelPadding);
			EstimatedFareLabel.SetVerticalCenter(16f);

			EtaLabel = new UILabel
			{
                AdjustsFontSizeToFitWidth = true,
				BackgroundColor = UIColor.Clear,
				Lines = 1,
                Font = UIFont.FromName(FontName.HelveticaNeueLight, 24 / 2),
                TextAlignment = NaturalLanguageHelper.GetTextAlignment(),
				TextColor = Theme.LabelTextColor,
				ShadowColor = UIColor.Clear,
			};

			EtaLabel.SetWidth(Frame.Width - VehicleLeftBadgeWidth - LabelPadding);
            EtaLabel.SetHeight(EtaContainerHeight);
			EtaLabel.SetHorizontalCenter((Frame.Width / 2) + (VehicleLeftBadgeWidth / 2) - LabelPadding);

			EtaContainer = new UIView (
				new RectangleF (0f, this.Frame.Height - EtaContainerHeight, 
					this.Frame.Width, EtaContainerHeight));


			EtaContainer.BackgroundColor = Theme.CompanyColor;
			VehicleSelection.Add (EtaContainer);
			EtaContainer.Add (EtaBadge = new VehicleTypeView (new RectangleF (0, 0, 0, 0)));

			AddSubviews(HorizontalDividerTop, EstimateSelectedVehicleType, EstimatedFareLabel, VehicleSelection);
        }

        public bool IsReadOnly { get; set; }

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

		private bool _showEta;
		public bool ShowEta
		{
			get { return _showEta; }
			set
			{
				if (_showEta != value)
				{
					_showEta = value;
					Redraw();
				}
			}
		}

		private bool _showVehicleSelection;
		public bool ShowVehicleSelection
		{
			get { return _showVehicleSelection; }
			set
			{
				if (_showVehicleSelection != value)
				{
					_showVehicleSelection = value;
					Redraw();
				}
			}
		}

        public VehicleType SelectedVehicle
        {
            get { return EstimateSelectedVehicleType.Vehicle; }
            set
            {
                if (EstimateSelectedVehicleType.Vehicle != value)
                {
                    EstimateSelectedVehicleType.Vehicle = value;
                    Redraw();
                }
            }
        }

        private IEnumerable<VehicleType> _vehicles = new List<VehicleType>();
        public IEnumerable<VehicleType> Vehicles
        {
            get { return _vehicles; }
            set
            {
                if (_vehicles != value)
                {
                    _vehicles = value;
                    Redraw();
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
                }
            }
        }

		public string Eta
		{
			get { return EtaLabel.Text; }
			set
			{
				if (EtaLabel.Text != value)
				{
					EtaLabel.Text = value;
					Redraw ();
				}
			}
		}

        private void Redraw()
        {
			bool showEta = !Eta.IsNullOrEmpty () && ShowEta;
			showEtaView (showEta);

			if (ShowEstimate)
            {
				BackgroundColor = Theme.CompanyColor;
                HorizontalDividerTop.BackgroundColor = Theme.LabelTextColor;
                EstimateSelectedVehicleType.Hidden = false;
                EstimatedFareLabel.Hidden = false;
				VehicleSelection.Hidden = true;
            }
            else
            {
				BackgroundColor = UIColor.Clear;
                HorizontalDividerTop.BackgroundColor = UIColor.FromRGB(177, 177, 177);
                EstimateSelectedVehicleType.Hidden = true;
                EstimatedFareLabel.Hidden = true;
                VehicleSelection.Hidden = false;
                VehicleSelection.Subviews.ForEach (x => x.RemoveFromSuperview ());

                if (Vehicles.None ())
                    return;

                var leftPadding = 16f;
                var width = (this.Frame.Width - leftPadding * 2) / Vehicles.Count ();
                var i = 0;

				VehicleType etaBadge = null;


				foreach (var vehicle in Vehicles) {
					var vehicleView = new VehicleTypeView (
						                   new RectangleF (leftPadding + i * width, 0f, width, this.Frame.Height), 
						                   vehicle, 
						                   SelectedVehicle != null ? vehicle.Id == SelectedVehicle.Id : false);

					if (etaBadge == null) {
						if (SelectedVehicle != null) {
							etaBadge = SelectedVehicle;
						} else {
							etaBadge = vehicle;
						}
					}

					vehicleView.TouchUpInside += (sender, e) => { 
						if (!IsReadOnly && VehicleSelected != null) {
							VehicleSelected (vehicle);
						}
					};

					VehicleSelection.Add (vehicleView);
					i++;

				}
					
				VehicleSelection.Add (EtaContainer);

				EtaBadge.RemoveFromSuperview ();
				EtaBadge = new VehicleTypeView (
                    new RectangleF (4f, (EtaContainerHeight - 40f) / 2, 40f, 40f), // use real size of image
					etaBadge,
					true,
					true);
				EtaContainer.Add (EtaBadge);
            }

			// Since this control doesn't use constraints:
            var badgeWidth = ShowEstimate ? EstimateSelectedVehicleType.WidthToFitLabel() : VehicleLeftBadgeWidth;
			EstimateSelectedVehicleType.SetWidth (badgeWidth);
			EstimatedFareLabel.SetWidth (this.Frame.Width - badgeWidth - LabelPadding * 2);
			EtaLabel.SetWidth (this.Frame.Width - badgeWidth - LabelPadding * 2);
			EtaLabel.SetX (badgeWidth + LabelPadding);
			EstimatedFareLabel.SetX (badgeWidth + LabelPadding);
		}

		private void showEtaView(bool showEta)
		{
			EtaLabel.RemoveFromSuperview ();
			VehicleSelection.Hidden = ShowEstimate;

			float etaTop = (!ShowVehicleSelection && !ShowEstimate) ? 0f : VehicleSelectionHeight;
			float etaHeight = etaTop + ((showEta && !ShowEstimate) ? EtaContainerHeight : 0f);
			EtaContainer.SetY (etaTop);
			this.SetHeight (etaHeight);
			_heightConstraint.Constant = etaHeight;

			if (Superview != null) {
				((OrderOptionsControl)Superview.Superview).Resize();
			}

			this.SetRoundedCorners(UIRectCorner.BottomLeft | UIRectCorner.BottomRight, 3f);
			EtaContainer.SetRoundedCorners(UIRectCorner.BottomLeft | UIRectCorner.BottomRight, 3f);

			if (!ShowEstimate && showEta) 
			{
				EtaContainer.Add (EtaLabel);
                EtaLabel.SetVerticalCenter(EtaContainerHeight / 2);
			}
				
			if (ShowEstimate) {
				if (showEta) {
					this.AddSubview (EtaLabel);
					EstimatedFareLabel.SetHeight(Frame.Height / 2);
					EstimatedFareLabel.SetVerticalCenter(16f);
					EstimatedFareLabel.Font = UIFont.FromName (FontName.HelveticaNeueLight, 32 / 2);
					EtaLabel.SetVerticalCenter(Frame.Height - 16f);
				} else {
					EstimatedFareLabel.SetHeight(Frame.Height - 10f);
					EstimatedFareLabel.SetVerticalCenter(Frame.Height / 2);
					EstimatedFareLabel.Font = UIFont.FromName (FontName.HelveticaNeueLight, 38 / 2);
				}
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
                        ((OrderOptionsControl)Superview.Superview).Resize();
                    }
                }
            }
        }
    }
}

