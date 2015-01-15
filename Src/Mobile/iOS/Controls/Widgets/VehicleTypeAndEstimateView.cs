using System;
using Foundation;
using UIKit;
using apcurium.MK.Booking.Mobile.Client.Style;
using System.Linq;
using apcurium.MK.Common.Extensions;
using CoreGraphics;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using apcurium.MK.Booking.Api.Contract.Resources;
using System.Collections.Generic;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("VehicleTypeAndEstimateView")]
    public class VehicleTypeAndEstimateView : UIView
    {
        private const float VehicleLeftBadgeWidth = 56f;
        private const float LabelPadding = 5f;
        private const float VehicleSelectionPadding = 16f;
        private const float EstimatedFareLabelTopValueWithEta = 2f;
        private const float EstimatedFareLabelTopValueWithoutEta = 0f;

        private UIView HorizontalDividerTop { get; set; }
        private VehicleTypeView EstimateSelectedVehicleType { get; set; }
        private UILabel EstimatedFareLabel { get; set; }
        private UIView VehicleSelectionContainer { get; set; }
        private UIView EstimateContainer { get; set; }
        private UILabel EtaLabel { get; set; }

        public Action<VehicleType> VehicleSelected { get; set; }

		private NSLayoutConstraint _constraintEstimateVehicleWidth;
        private NSLayoutConstraint _constraintEstimatedFareLabelHeight;
        private NSLayoutConstraint _constraintEstimatedFareLabelTop;
        private NSLayoutConstraint _estimatedFareLabelHeightValueWithEta;
        private NSLayoutConstraint _estimatedFareLabelHeightValueWithoutEta;

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
            HorizontalDividerTop = Line.CreateHorizontal(Frame.Width, Theme.LabelTextColor);

            VehicleSelectionContainer = new UIView ();
            VehicleSelectionContainer.TranslatesAutoresizingMaskIntoConstraints = false;

            EstimateContainer = new UIView ();
            EstimateContainer.TranslatesAutoresizingMaskIntoConstraints = false;

            AddSubviews(HorizontalDividerTop, VehicleSelectionContainer, EstimateContainer);

            // Constraints for VehicleSelectionContainer
            AddConstraints(new [] 
            { 
                NSLayoutConstraint.Create(VehicleSelectionContainer, NSLayoutAttribute.Left, NSLayoutRelation.Equal, VehicleSelectionContainer.Superview, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(VehicleSelectionContainer, NSLayoutAttribute.Right, NSLayoutRelation.Equal, VehicleSelectionContainer.Superview, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(VehicleSelectionContainer, NSLayoutAttribute.Top, NSLayoutRelation.Equal, VehicleSelectionContainer.Superview, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(VehicleSelectionContainer, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, VehicleSelectionContainer.Superview, NSLayoutAttribute.Bottom, 1f, 0f)
            });

            // Constraints for EstimateContainer
            AddConstraints(new [] 
            { 
                NSLayoutConstraint.Create(EstimateContainer, NSLayoutAttribute.Left, NSLayoutRelation.Equal, EstimateContainer.Superview, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(EstimateContainer, NSLayoutAttribute.Right, NSLayoutRelation.Equal, EstimateContainer.Superview, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(EstimateContainer, NSLayoutAttribute.Top, NSLayoutRelation.Equal, EstimateContainer.Superview, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(EstimateContainer, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, EstimateContainer.Superview, NSLayoutAttribute.Bottom, 1f, 0f)
            });

            InitializeEstimateContainer();
        }

        private void InitializeEstimateContainer()
        {
            EstimateSelectedVehicleType = new VehicleTypeView(new CGRect(0f, 0f, 50f, this.Frame.Height));
            EstimateSelectedVehicleType.TranslatesAutoresizingMaskIntoConstraints = false;

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
            EstimatedFareLabel.TranslatesAutoresizingMaskIntoConstraints = false;

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
            EtaLabel.TranslatesAutoresizingMaskIntoConstraints = false;

            EstimateContainer.AddSubviews(EstimateSelectedVehicleType, EstimatedFareLabel, EtaLabel);

            // initialize constraint values
            _estimatedFareLabelHeightValueWithEta = NSLayoutConstraint.Create(EstimatedFareLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, EstimatedFareLabel.Superview, NSLayoutAttribute.Height, 0.5f, 0f);
            _estimatedFareLabelHeightValueWithoutEta = NSLayoutConstraint.Create(EstimatedFareLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, EstimatedFareLabel.Superview, NSLayoutAttribute.Height, 1f, 0f);
            _constraintEstimatedFareLabelTop = NSLayoutConstraint.Create(EstimatedFareLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, EstimatedFareLabel.Superview, NSLayoutAttribute.Top, 1f, EstimatedFareLabelTopValueWithoutEta);

            // Constraints for EstimateSelectedVehicleType
            _constraintEstimateVehicleWidth = NSLayoutConstraint.Create(EstimateSelectedVehicleType, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, 40f);
            EstimateContainer.AddConstraints(new [] 
            { 
                _constraintEstimateVehicleWidth,
                NSLayoutConstraint.Create(EstimateSelectedVehicleType, NSLayoutAttribute.Height, NSLayoutRelation.Equal, EstimateSelectedVehicleType.Superview, NSLayoutAttribute.Height, 1f, 0f),
                NSLayoutConstraint.Create(EstimateSelectedVehicleType, NSLayoutAttribute.Top, NSLayoutRelation.Equal, EstimateSelectedVehicleType.Superview, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(EstimateSelectedVehicleType, NSLayoutAttribute.Left, NSLayoutRelation.Equal, EstimateSelectedVehicleType.Superview, NSLayoutAttribute.Left, 1f, 0f)
            });

            // Constraints for EstimatedFareLabel
            _constraintEstimatedFareLabelHeight = _estimatedFareLabelHeightValueWithoutEta;
            EstimateContainer.AddConstraints(new [] 
            { 
                NSLayoutConstraint.Create(EstimatedFareLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, EstimateSelectedVehicleType, NSLayoutAttribute.Right, 1f, LabelPadding),
                NSLayoutConstraint.Create(EstimatedFareLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, EstimatedFareLabel.Superview, NSLayoutAttribute.Right, 1f, -LabelPadding),
                _constraintEstimatedFareLabelHeight,
                _constraintEstimatedFareLabelTop
            });

            // Constraints for EstimatedFareLabel
            EstimateContainer.AddConstraints(new [] 
            { 
                NSLayoutConstraint.Create(EtaLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, EstimateSelectedVehicleType, NSLayoutAttribute.Right, 1f, LabelPadding),
                NSLayoutConstraint.Create(EtaLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, EstimatedFareLabel.Superview, NSLayoutAttribute.Right, 1f, -LabelPadding),
                NSLayoutConstraint.Create(EtaLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, EstimatedFareLabel.Superview, NSLayoutAttribute.Height, 0.5f, 0f),
                NSLayoutConstraint.Create(EtaLabel, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, EstimatedFareLabel.Superview, NSLayoutAttribute.Bottom, 1f, -2f)
            });
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            this.SetRoundedCorners(UIRectCorner.BottomLeft | UIRectCorner.BottomRight, ShowEstimate ? 3f : 0f);
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
			if (ShowEstimate)
            {
				BackgroundColor = Theme.CompanyColor;
                HorizontalDividerTop.BackgroundColor = Theme.LabelTextColor;
                EstimateContainer.Hidden = false;
				VehicleSelectionContainer.Hidden = true;

                if (Eta.HasValue())
                {
                    EstimateContainer.RemoveConstraint(_constraintEstimatedFareLabelHeight);
                    _constraintEstimatedFareLabelHeight = _estimatedFareLabelHeightValueWithEta;
                    EstimateContainer.AddConstraint(_constraintEstimatedFareLabelHeight);
                    _constraintEstimatedFareLabelTop.Constant = EstimatedFareLabelTopValueWithEta;
                }
                else
                {
                    EstimateContainer.RemoveConstraint(_constraintEstimatedFareLabelHeight);
                    _constraintEstimatedFareLabelHeight = _estimatedFareLabelHeightValueWithoutEta;
                    EstimateContainer.AddConstraint(_constraintEstimatedFareLabelHeight);
                    _constraintEstimatedFareLabelTop.Constant = EstimatedFareLabelTopValueWithoutEta;
                }
            }
            else
            {
				BackgroundColor = UIColor.Clear;
                HorizontalDividerTop.BackgroundColor = UIColor.FromRGB(177, 177, 177);
                EstimateContainer.Hidden = true;
                VehicleSelectionContainer.Hidden = false;

                VehicleSelectionContainer.Subviews.ForEach(x => x.RemoveFromSuperview());

                if (Vehicles.None())
                {
                    return;
                }

                var i = 0;
				foreach (var vehicle in Vehicles) 
                {
                    var vehicleView = new VehicleTypeView(new CGRect(), vehicle, SelectedVehicle != null ? vehicle.Id == SelectedVehicle.Id : false);
                    vehicleView.TranslatesAutoresizingMaskIntoConstraints = false;
					vehicleView.TouchUpInside += (sender, e) => { 
						if (!IsReadOnly && VehicleSelected != null) {
							VehicleSelected (vehicle);
						}
					};

					VehicleSelectionContainer.Add (vehicleView);

                    VehicleSelectionContainer.AddConstraints(new [] 
                    { 
                        NSLayoutConstraint.Create(vehicleView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, vehicleView.Superview, NSLayoutAttribute.Top, 1f, 0f),
                        NSLayoutConstraint.Create(vehicleView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, vehicleView.Superview, NSLayoutAttribute.Height, 1f, 0f)
                    });

                    if (i == 0)
                    {
                        // first vehicle
                        VehicleSelectionContainer.AddConstraint(NSLayoutConstraint.Create(vehicleView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, vehicleView.Superview, NSLayoutAttribute.Left, 1f, VehicleSelectionPadding));
                    }
                    else
                    {
                        // add constraint relative to previous vehicle
                        VehicleSelectionContainer.AddConstraints(new [] 
                        {
                            NSLayoutConstraint.Create(vehicleView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, VehicleSelectionContainer.Subviews[i - 1], NSLayoutAttribute.Right, 1f, 0f),
                            NSLayoutConstraint.Create(vehicleView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, VehicleSelectionContainer.Subviews[i - 1], NSLayoutAttribute.Width, 1f, 0f)
                        });
                    }

                    if (i == (Vehicles.Count() - 1))
                    {
                        // last vehicle
                        VehicleSelectionContainer.AddConstraint(NSLayoutConstraint.Create(vehicleView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, VehicleSelectionContainer, NSLayoutAttribute.Right, 1f, -VehicleSelectionPadding));
                    }

					i++;
				}
            }
                
            _constraintEstimateVehicleWidth.Constant = EstimateSelectedVehicleType.WidthToFitLabel();
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

