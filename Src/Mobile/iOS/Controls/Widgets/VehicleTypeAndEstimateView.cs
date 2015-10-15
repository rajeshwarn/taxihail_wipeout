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
		private const float ExpandedBaseRateControlHeight = 100f;
		private const float EstimatedFareControlHeight = 20f;

        private UIView HorizontalDividerTop { get; set; }
        private VehicleTypeView EstimateSelectedVehicleType { get; set; }
        private UILabel EstimatedFareLabel { get; set; }
        private UIView VehicleSelectionContainer { get; set; }
        private UIView EstimateContainer { get; set; }
        private UILabel EtaLabel { get; set; }
		private BaseRateToggleView BaseRateToggle { get; set; }

        public Action<OrderOptionsViewModel.VehicleSelectionModel> VehicleSelected { get; set; }
		
        private NSLayoutConstraint _constraintControlHeight;
		private NSLayoutConstraint _constraintEstimateVehicleWidth;
        private NSLayoutConstraint _constraintEstimatedFareLabelHeight;
        private NSLayoutConstraint _estimatedFareLabelHeightValueWithEta;
        private NSLayoutConstraint _estimatedFareLabelHeightValueWithoutEta;
		private NSLayoutConstraint _vehicleSelectionContainerHeight;

        private bool BaseRateEnabled { get; set; }

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

            //DEBUG
			BaseRateEnabled = true;


            _constraintControlHeight = NSLayoutConstraint.Create(this, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, DefaultControlHeight);
            AddConstraint(_constraintControlHeight);

            HorizontalDividerTop = Line.CreateHorizontal(Frame.Width, Theme.LabelTextColor);

            VehicleSelectionContainer = new UIView 
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
				//BackgroundColor = Theme.LabelTextColor,
				BackgroundColor = UIColor.Red
            };

            EstimateContainer = new UIView 
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            AddSubviews(HorizontalDividerTop, VehicleSelectionContainer, EstimateContainer);

            // Constraints for VehicleSelectionContainer
			_vehicleSelectionContainerHeight = NSLayoutConstraint.Create(VehicleSelectionContainer, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.Height, 1f, GroupVehiclesByServiceType ? GroupByServiceTypeControlHeight : DefaultControlHeight);

            AddConstraints(new [] 
            { 
                NSLayoutConstraint.Create(VehicleSelectionContainer, NSLayoutAttribute.Left, NSLayoutRelation.Equal, VehicleSelectionContainer.Superview, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(VehicleSelectionContainer, NSLayoutAttribute.Right, NSLayoutRelation.Equal, VehicleSelectionContainer.Superview, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(VehicleSelectionContainer, NSLayoutAttribute.Top, NSLayoutRelation.Equal, VehicleSelectionContainer.Superview, NSLayoutAttribute.Top, 1f, 0f),
				_vehicleSelectionContainerHeight
            });

            InitializeEstimateContainer();
        }

        private void InitializeEstimateContainer ()
		{
			// Constraints for EstimateContainer
			AddConstraints (new [] { 
				NSLayoutConstraint.Create (EstimateContainer, NSLayoutAttribute.Left, NSLayoutRelation.Equal, EstimateContainer.Superview, NSLayoutAttribute.Left, 1f, 0f),
				NSLayoutConstraint.Create (EstimateContainer, NSLayoutAttribute.Right, NSLayoutRelation.Equal, EstimateContainer.Superview, NSLayoutAttribute.Right, 1f, 0f),
				NSLayoutConstraint.Create (EstimateContainer, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, EstimateContainer.Superview, NSLayoutAttribute.Bottom, 1f, 0f),
				NSLayoutConstraint.Create (EstimateContainer, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, EstimatedFareControlHeight)
			});

			BaseRateToggle = new BaseRateToggleView ();
			BaseRateToggle.ToggleBaseRate = () => ShowBaseRate = BaseRateToggle.Toggle();

			EstimateSelectedVehicleType = new VehicleTypeView (new CGRect (0f, 0f, 50f, this.Frame.Height));

			EstimatedFareLabel = new UILabel {
				TranslatesAutoresizingMaskIntoConstraints = false,
				AdjustsFontSizeToFitWidth = true,
				Lines = 1,
				Font = UIFont.FromName (FontName.HelveticaNeueLight, 32 / 2),
				TextAlignment = NaturalLanguageHelper.GetTextAlignment (),
				TextColor = Theme.LabelTextColor,
				ShadowColor = UIColor.Clear,
				BackgroundColor = UIColor.Green
			};

			EtaLabel = new UILabel {
				TranslatesAutoresizingMaskIntoConstraints = false,
				AdjustsFontSizeToFitWidth = true,
				Lines = 1,
				Font = UIFont.FromName (FontName.HelveticaNeueLight, 24 / 2),
				TextAlignment = NaturalLanguageHelper.GetTextAlignment (),
				TextColor = Theme.LabelTextColor,
				ShadowColor = UIColor.Clear,
			};

			EstimateContainer.AddSubviews (EstimatedFareLabel, EtaLabel);

			if (!BaseRateEnabled) // SelectedVehicle not shown when in baserate mode
			{
				EstimateContainer.AddSubview (EstimateSelectedVehicleType);

				// Constraints for EtaLabel
				EstimateContainer.AddConstraints (new [] { 
					NSLayoutConstraint.Create (EtaLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, EstimateSelectedVehicleType, NSLayoutAttribute.Right, 1f, LabelPadding),
					NSLayoutConstraint.Create (EtaLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, EstimatedFareLabel.Superview, NSLayoutAttribute.Right, 1f, -LabelPadding),
					NSLayoutConstraint.Create (EtaLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, EstimatedFareLabel.Superview, NSLayoutAttribute.Height, 0.5f, 0f),
					NSLayoutConstraint.Create (EtaLabel, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, EstimatedFareLabel.Superview, NSLayoutAttribute.Bottom, 1f, -2f)
				});

				// Constraints for EstimateSelectedVehicleType
				_constraintEstimateVehicleWidth = NSLayoutConstraint.Create (EstimateSelectedVehicleType, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, 40f);
				EstimateContainer.AddConstraints (new [] { 
					_constraintEstimateVehicleWidth,
					NSLayoutConstraint.Create (EstimateSelectedVehicleType, NSLayoutAttribute.Height, NSLayoutRelation.Equal, EstimateSelectedVehicleType.Superview, NSLayoutAttribute.Height, 1f, 0f),
					NSLayoutConstraint.Create (EstimateSelectedVehicleType, NSLayoutAttribute.Top, NSLayoutRelation.Equal, EstimateSelectedVehicleType.Superview, NSLayoutAttribute.Top, 1f, 0f),
					NSLayoutConstraint.Create (EstimateSelectedVehicleType, NSLayoutAttribute.Left, NSLayoutRelation.Equal, EstimateSelectedVehicleType.Superview, NSLayoutAttribute.Left, 1f, 0f)
				});
			} else
			{
				EstimateContainer.AddSubview (BaseRateToggle);

				EstimateContainer.AddConstraints (new [] { 
					NSLayoutConstraint.Create (BaseRateToggle, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, 32f),
					NSLayoutConstraint.Create (BaseRateToggle, NSLayoutAttribute.Right, NSLayoutRelation.Equal, BaseRateToggle.Superview, NSLayoutAttribute.Right, 1f, 0f),
					NSLayoutConstraint.Create (BaseRateToggle, NSLayoutAttribute.Top, NSLayoutRelation.Equal, BaseRateToggle.Superview, NSLayoutAttribute.Top, 1f, 0f),
					NSLayoutConstraint.Create (BaseRateToggle, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, BaseRateToggle.Superview, NSLayoutAttribute.Bottom, 1f, 0f)
				});
			}

			// initialize constraint values
			_estimatedFareLabelHeightValueWithEta = NSLayoutConstraint.Create (EstimatedFareLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, EstimatedFareLabel.Superview, NSLayoutAttribute.Height, 0.5f, 0f);

			_estimatedFareLabelHeightValueWithoutEta = !BaseRateEnabled ? 
				NSLayoutConstraint.Create (EstimatedFareLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, EstimatedFareLabel.Superview, NSLayoutAttribute.Height, 1f, 0f) :
				NSLayoutConstraint.Create (EstimatedFareLabel, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, EstimatedFareControlHeight);

			var estimatedFareLabelLeft = !BaseRateEnabled ?
					NSLayoutConstraint.Create (EstimatedFareLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, EstimateSelectedVehicleType, NSLayoutAttribute.Right, 1f, LabelPadding)
				: NSLayoutConstraint.Create (EstimatedFareLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, EstimatedFareLabel.Superview, NSLayoutAttribute.Left, 1f, LabelPadding);

			var estimatedFareLabelRight = !BaseRateEnabled ?
					NSLayoutConstraint.Create (EstimatedFareLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, EstimatedFareLabel.Superview, NSLayoutAttribute.Right, 1f, -LabelPadding)
				: NSLayoutConstraint.Create (EstimatedFareLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, BaseRateToggle, NSLayoutAttribute.Left, 1f, 0f);

			// Constraints for EstimatedFareLabel
			_constraintEstimatedFareLabelHeight = _estimatedFareLabelHeightValueWithoutEta;

			EstimateContainer.AddConstraints (new [] { 
				estimatedFareLabelLeft,
				estimatedFareLabelRight,
				_constraintEstimatedFareLabelHeight,
				NSLayoutConstraint.Create (EstimatedFareLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, EstimatedFareLabel.Superview, NSLayoutAttribute.Top, 1f, EstimatedFareLabelTopValueWithoutEta)
			});
        }

        private void Redraw ()
		{
			VehicleSelectionContainer.RemoveConstraint(_vehicleSelectionContainerHeight);
			_vehicleSelectionContainerHeight = NSLayoutConstraint.Create(VehicleSelectionContainer, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.Height, 1f, GroupVehiclesByServiceType ? GroupByServiceTypeControlHeight : DefaultControlHeight);
			VehicleSelectionContainer.AddConstraint(_vehicleSelectionContainerHeight);

			if (ShowEstimate)
			{
				BackgroundColor = BaseRateEnabled ? UIColor.Black : Theme.CompanyColor;
				HorizontalDividerTop.BackgroundColor = Theme.LabelTextColor;
				EstimateContainer.Hidden = false;

				VehicleSelectionContainer.Hidden = !BaseRateEnabled;
				if (BaseRateEnabled)
				{
					DrawVehicleSelectionContainerContent ();
				}

				ChangeControlHeightIfNeeded (true);

				if (Eta.HasValue () && ShowEta)
				{
					EstimateContainer.RemoveConstraint (_constraintEstimatedFareLabelHeight);
					_constraintEstimatedFareLabelHeight = _estimatedFareLabelHeightValueWithEta;
					EstimateContainer.AddConstraint (_constraintEstimatedFareLabelHeight);
				} else
				{
					EstimateContainer.RemoveConstraint (_constraintEstimatedFareLabelHeight);
					_constraintEstimatedFareLabelHeight = _estimatedFareLabelHeightValueWithoutEta;
					EstimateContainer.AddConstraint (_constraintEstimatedFareLabelHeight);
				}
			} else
			{
				BackgroundColor = UIColor.Clear;
				BackgroundColor = UIColor.Orange;
				HorizontalDividerTop.BackgroundColor = UIColor.FromRGB (177, 177, 177);
				EstimateContainer.Hidden = false;
				VehicleSelectionContainer.Hidden = false;

				DrawVehicleSelectionContainerContent ();
			}

			if (!BaseRateEnabled)
			{
				_constraintEstimateVehicleWidth.Constant = EstimateSelectedVehicleType.WidthToFitLabel();
			}
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
                        BackgroundColor = UIColor.FromRGB(227, 227, 227)
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

        private void ChangeControlHeightIfNeeded (bool setToDefault)
		{
			var wantedValue = (setToDefault && !BaseRateEnabled) ? DefaultControlHeight : GroupByServiceTypeControlHeight;

			if (BaseRateEnabled)
			{
				wantedValue += EstimatedFareControlHeight;
			}

			if (ShowEstimate && ShowBaseRate)
			{
				wantedValue += ExpandedBaseRateControlHeight;
			}

            if (_constraintControlHeight.Constant != wantedValue)
            {
                _constraintControlHeight.Constant = wantedValue;

                // force the constraint to be applied and the bounds to be updated immediately
                SetNeedsLayout();
                LayoutIfNeeded();

                ((OrderOptionsControl)Superview.Superview).Resize();
				Console.WriteLine(((OrderOptionsControl)Superview.Superview).Frame.Height);
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

        public bool IsReadOnly 
        { 
        get; 
        set; 
        }

        private bool _showBaseRate;
        public bool ShowBaseRate {
        	get { 
        		return _showBaseRate; 
        	}
        	set {
        		_showBaseRate = value;
        		Redraw();
        	}
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

	public class BaseRateToggleView : UIButton 
    {
    	public BaseRateToggleView ()
		{
			TranslatesAutoresizingMaskIntoConstraints = false;
			Font = UIFont.FromName (FontName.HelveticaNeueLight, 32 / 2);
			ContentMode = UIViewContentMode.Center;
			SetTitleColor(Theme.LabelTextColor, UIControlState.Normal);
			SetTitle("∨", UIControlState.Normal);
			Enabled = true;

			TouchUpInside += (object sender, EventArgs e) => {
				ToggleBaseRate.Invoke();
			};
		}

    	public bool Toggle() 
    	{
			Expanded = !Expanded;
			SetTitle(Expanded ? "∧" : "∨", UIControlState.Normal);
			return Expanded;
    	}

    	public Func<bool> ToggleBaseRate {get;set;}

		public bool Expanded { get; set; }
	}
}

