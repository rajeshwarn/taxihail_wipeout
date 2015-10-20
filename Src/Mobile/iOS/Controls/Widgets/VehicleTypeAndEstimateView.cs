using System;
using Foundation;
using UIKit;
using apcurium.MK.Booking.Mobile.Client.Style;
using System.Linq;
using apcurium.MK.Common.Extensions;
using CoreGraphics;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using apcurium.MK.Booking.Api.Contract.Resources;
using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("VehicleTypeAndEstimateView")]
    public class VehicleTypeAndEstimateView : UIView
    {
        private const float VehicleLeftBadgeWidth = 56f;
        private const float LabelPadding = 5f;
        private const float VehicleSelectionPadding = 16f;
        private const float VehicleSubSelectionPadding = 12f;
        private const float EstimatedFareLabelTopValueWithEta = 2f;
        private const float EstimatedFareLabelTopValueWithoutEta = 0f;
        private const float DefaultControlHeight = 52f;
        private const float GroupByServiceTypeControlHeight = 82f;

        private UIView HorizontalDividerTop { get; set; }
        private VehicleTypeView EstimateSelectedVehicleType { get; set; }
        private UILabel EstimatedFareLabel { get; set; }
        private UIView VehicleSelectionContainer { get; set; }
        private UIView EstimateContainer { get; set; }
        private UILabel EtaLabel { get; set; }

        public Action<OrderOptionsViewModel.VehicleSelectionModel> VehicleSelected { get; set; }

        private NSLayoutConstraint _constraintControlHeight;
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
            VehicleRepresentations = Vehicles.ToList();

            _constraintControlHeight = NSLayoutConstraint.Create(this, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, DefaultControlHeight);
            AddConstraint(_constraintControlHeight);

            HorizontalDividerTop = Line.CreateHorizontal(Frame.Width, Theme.LabelTextColor);

            VehicleSelectionContainer = new UIView 
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = UIColor.FromRGB(227, 227, 227)
            };

            EstimateContainer = new UIView 
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            AddSubviews(HorizontalDividerTop, VehicleSelectionContainer, EstimateContainer);

            // Constraints for VehicleSelectionContainer
            AddConstraints(new [] 
            { 
                NSLayoutConstraint.Create(VehicleSelectionContainer, NSLayoutAttribute.Left, NSLayoutRelation.Equal, VehicleSelectionContainer.Superview, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(VehicleSelectionContainer, NSLayoutAttribute.Right, NSLayoutRelation.Equal, VehicleSelectionContainer.Superview, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(VehicleSelectionContainer, NSLayoutAttribute.Top, NSLayoutRelation.Equal, VehicleSelectionContainer.Superview, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(VehicleSelectionContainer, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, VehicleSelectionContainer.Superview, NSLayoutAttribute.Bottom, 1f, 0f)
            });

            InitializeEstimateContainer();
        }

        private void InitializeEstimateContainer()
        {
            // Constraints for EstimateContainer
            AddConstraints(new [] 
            { 
                NSLayoutConstraint.Create(EstimateContainer, NSLayoutAttribute.Left, NSLayoutRelation.Equal, EstimateContainer.Superview, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(EstimateContainer, NSLayoutAttribute.Right, NSLayoutRelation.Equal, EstimateContainer.Superview, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(EstimateContainer, NSLayoutAttribute.Top, NSLayoutRelation.Equal, EstimateContainer.Superview, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(EstimateContainer, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, EstimateContainer.Superview, NSLayoutAttribute.Bottom, 1f, 0f)
            });

            EstimateSelectedVehicleType = new VehicleTypeView(new CGRect(0f, 0f, 50f, this.Frame.Height));

            EstimatedFareLabel = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                AdjustsFontSizeToFitWidth = true,
                BackgroundColor = UIColor.Clear,
                Lines = 1,
                Font = UIFont.FromName(FontName.HelveticaNeueLight, 32 / 2),
                TextAlignment = NaturalLanguageHelper.GetTextAlignment(),
                TextColor = Theme.LabelTextColor,
                ShadowColor = UIColor.Clear
            };

            EtaLabel = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                AdjustsFontSizeToFitWidth = true,
                BackgroundColor = UIColor.Clear,
                Lines = 1,
                Font = UIFont.FromName(FontName.HelveticaNeueLight, 24 / 2),
                TextAlignment = NaturalLanguageHelper.GetTextAlignment(),
                TextColor = Theme.LabelTextColor,
                ShadowColor = UIColor.Clear,
            };

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

        private void Redraw()
        {
			if (ShowEstimate)
            {
				BackgroundColor = Theme.CompanyColor;
                HorizontalDividerTop.BackgroundColor = Theme.LabelTextColor;
                EstimateContainer.Hidden = false;
				VehicleSelectionContainer.Hidden = true;
                ChangeControlHeightIfNeeded(true);

                if (Eta.HasValue() && ShowEta)
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

                DrawVehicleSelectionContainerContent();
            }
                
            _constraintEstimateVehicleWidth.Constant = EstimateSelectedVehicleType.WidthToFitLabel();
		}

        private void DrawVehicleForMainSelector(VehicleType vehicle, int index, bool isFirst, bool isLast)
        {
            var currentIsSelected = IsVehicleSelected(vehicle);

            var vehicleView = new VehicleTypeView(new CGRect(), vehicle, currentIsSelected);
            vehicleView.TouchUpInside += (sender, e) => { 
                if (VehicleSelected != null) 
                {
                    VehicleSelected(new OrderOptionsViewModel.VehicleSelectionModel { VehicleType = vehicle, IsSubMenuSelection = false });
                }
            };

            VehicleSelectionContainer.AddSubview (vehicleView);

            VehicleSelectionContainer.AddConstraints(new [] 
            { 
                NSLayoutConstraint.Create(vehicleView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, vehicleView.Superview, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(vehicleView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, DefaultControlHeight)
            });

            if (isFirst)
            {
                // first vehicle
                VehicleSelectionContainer.AddConstraint(NSLayoutConstraint.Create(vehicleView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, vehicleView.Superview, NSLayoutAttribute.Left, 1f, VehicleSelectionPadding));
            }
            else
            {
                // add constraint relative to previous vehicle
                VehicleSelectionContainer.AddConstraints(new [] 
                {
                    NSLayoutConstraint.Create(vehicleView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, vehicleView.Superview.Subviews[index - 1], NSLayoutAttribute.Right, 1f, 0f),
                    NSLayoutConstraint.Create(vehicleView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, vehicleView.Superview.Subviews[index - 1], NSLayoutAttribute.Width, 1f, 0f)
                });
            }

            if (isLast)
            {
                // last vehicle
                VehicleSelectionContainer.AddConstraint(NSLayoutConstraint.Create(vehicleView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, vehicleView.Superview, NSLayoutAttribute.Right, 1f, -VehicleSelectionPadding));
            }

            if (GroupVehiclesByServiceType && currentIsSelected)
            {
                var vehiclesForThisServiceType = Vehicles.Where(x => x.ServiceType == SelectedVehicle.ServiceType).ToList();
                if (vehiclesForThisServiceType.Count > 1)
                {
                    // the sub-selection is needed
                    var subSelectionView = new UIView 
                    { 
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        BackgroundColor = UIColor.FromRGB(240, 240, 240)
                    };
                    VehicleSelectionContainer.AddSubview(subSelectionView);

                    VehicleSelectionContainer.AddConstraints(new [] 
                    {
                        NSLayoutConstraint.Create(subSelectionView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, subSelectionView.Superview, NSLayoutAttribute.Top, 1f, DefaultControlHeight),
                        NSLayoutConstraint.Create(subSelectionView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.Width, 1f, GroupByServiceTypeControlHeight - DefaultControlHeight),
                        NSLayoutConstraint.Create(subSelectionView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, subSelectionView.Superview, NSLayoutAttribute.Left, 1f, 0f),
                        NSLayoutConstraint.Create(subSelectionView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, subSelectionView.Superview, NSLayoutAttribute.Right, 1f, 0f)
                    });

                    var y = 0;
                    foreach (var vehicleType in vehiclesForThisServiceType)
                    {
                        DrawVehicleForSubSelector(subSelectionView, vehicleType, y, y == 0, y == (vehiclesForThisServiceType.Count - 1));
                        y++;
                    }

                    ChangeControlHeightIfNeeded(false);
                }
                else
                {
                    ChangeControlHeightIfNeeded(true);
                }
            }
        }

        private void DrawVehicleForSubSelector(UIView subSelectorContainer, VehicleType vehicle, int index, bool isFirst, bool isLast)
        {
            var vehicleSubView = new VehicleTypeSubView(new CGRect(), vehicle, IsVehicleSelected(vehicle, true));
            vehicleSubView.TouchUpInside += (sender, e) => { 
                if (VehicleSelected != null) 
                {
                    VehicleSelected(new OrderOptionsViewModel.VehicleSelectionModel { VehicleType = vehicle, IsSubMenuSelection = true });
                }
            };

            subSelectorContainer.AddSubview(vehicleSubView);

            subSelectorContainer.AddConstraint(NSLayoutConstraint.Create(vehicleSubView, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, vehicleSubView.Superview, NSLayoutAttribute.CenterY, 1f, 0f));
            subSelectorContainer.AddConstraint(NSLayoutConstraint.Create(vehicleSubView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, 15f));

            if (isFirst)
            {
                // first vehicle
                subSelectorContainer.AddConstraint(NSLayoutConstraint.Create(vehicleSubView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, vehicleSubView.Superview, NSLayoutAttribute.Left, 1f, VehicleSubSelectionPadding));
            }
            else
            {
                // add constraint relative to previous vehicle
                subSelectorContainer.AddConstraints(new [] 
                {
                    NSLayoutConstraint.Create(vehicleSubView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, vehicleSubView.Superview.Subviews[index - 1], NSLayoutAttribute.Right, 1f, 0f),
                    NSLayoutConstraint.Create(vehicleSubView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, vehicleSubView.Superview.Subviews[index - 1], NSLayoutAttribute.Width, 1f, 0f)
                });
            }

            if (isLast)
            {
                // last vehicle
                subSelectorContainer.AddConstraint(NSLayoutConstraint.Create(vehicleSubView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, vehicleSubView.Superview, NSLayoutAttribute.Right, 1f, -VehicleSubSelectionPadding));
            }
        }

        private void DrawVehicleSelectionContainerContent()
        {
            VehicleSelectionContainer.Subviews.ForEach(x => x.RemoveFromSuperview());

            if (Vehicles.None())
            {
                return;
            }

            var i = 0;
            foreach (var vehicle in VehicleRepresentations) 
            {
                DrawVehicleForMainSelector(vehicle, i, i == 0, i == (VehicleRepresentations.Count - 1));
                i++;
            }
        }

        private bool IsVehicleSelected(VehicleType vehicle, bool isForSubSelection = false)
        {
            return SelectedVehicle != null && ((isForSubSelection || !GroupVehiclesByServiceType) 
                ? vehicle.Id == SelectedVehicle.Id 
                : vehicle.ServiceType == SelectedVehicle.ServiceType);
        }

        private void ChangeControlHeightIfNeeded(bool setToDefault)
        {
            var wantedValue = setToDefault ? DefaultControlHeight : GroupByServiceTypeControlHeight;
            if (_constraintControlHeight.Constant != wantedValue)
            {
                _constraintControlHeight.Constant = wantedValue;

                // force the constraint to be applied and the bounds to be updated immediately
                SetNeedsLayout();
                LayoutIfNeeded();

                ((OrderOptionsControl)Superview.Superview).Resize();
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

        public IList<VehicleType> VehicleRepresentations { get; set; }

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

        private bool _showEta;
        public bool ShowEta
        {
            get
            {
                return _showEta;
            }
            set
            {
                _showEta = value;

                EtaLabel.Hidden = !value;

                Redraw();
            }
        }

        private bool _groupVehiclesByServiceType;
        public bool GroupVehiclesByServiceType
        {
            get
            {
                return _groupVehiclesByServiceType;
            }
            set
            {
                _groupVehiclesByServiceType = value;

                Redraw();
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
    }
}

