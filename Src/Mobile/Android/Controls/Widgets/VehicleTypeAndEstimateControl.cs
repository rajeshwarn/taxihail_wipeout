using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Api.Contract.Resources;
using System.Collections.Generic;
using System;
using apcurium.MK.Common.Extensions;
using Android.Runtime;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using System.Linq;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("apcurium.mk.booking.mobile.client.controls.widgets.VehicleTypeAndEstimateControl")]
    public class VehicleTypeAndEstimateControl : LinearLayout
    {
		private AutoResizeTextView _estimatedFareLabel;
		private AutoResizeTextView _etaLabel;
		private View _horizontalDivider;
		private LinearLayout _vehicleSelectionAndEta;
		private LinearLayout _vehicleSelection;
        private LinearLayout _vehicleSubSelection;
		private LinearLayout _rideEstimate;
		private VehicleTypeControl _estimateSelectedVehicleType;

        public Action<OrderOptionsViewModel.VehicleSelectionModel> VehicleSelected { get; set; }

        public VehicleTypeAndEstimateControl(Context c, IAttributeSet attr) : base(c, attr)
        {
        }

        protected override void OnFinishInflate()
        {
            base.OnFinishInflate();
            var inflater = (LayoutInflater) Context.GetSystemService(Context.LayoutInflaterService);
            var layout = inflater.Inflate(Resource.Layout.Control_VehicleTypeAndEstimate, this, true);

			_horizontalDivider = (View)layout.FindViewById(Resource.Id.HorizontalDivider);
			_rideEstimate = (LinearLayout)layout.FindViewById (Resource.Id.RideEstimate);
			_vehicleSelectionAndEta = (LinearLayout)layout.FindViewById (Resource.Id.VehicleSelectionAndEta);
			_vehicleSelection = (LinearLayout)layout.FindViewById (Resource.Id.VehicleSelection);
            _vehicleSubSelection = (LinearLayout)layout.FindViewById (Resource.Id.VehicleSubSelection);

            _estimatedFareLabel = (AutoResizeTextView)layout.FindViewById(Resource.Id.estimateFareAutoResizeLabel);
			
			_etaLabel = (AutoResizeTextView)layout.FindViewById(Resource.Id.EtaLabel);

			_estimateSelectedVehicleType = (VehicleTypeControl)layout.FindViewById (Resource.Id.estimateSelectedVehicle);
            _estimateSelectedVehicleType.Selected = true;

            this.SetBackgroundColorWithRoundedCorners(0, 0, 3, 3, Resources.GetColor(Resource.Color.company_color));
        }
		
        private void Redraw()
        {
			if (ShowEstimate)
            {
				this.SetBackgroundColorWithRoundedCorners(0, 0, 3, 3, Resources.GetColor(Resource.Color.company_color));
                _horizontalDivider.Background.SetColorFilter(Resources.GetColor(Resource.Color.company_color), PorterDuff.Mode.SrcAtop);
				_rideEstimate.Visibility = ViewStates.Visible;
				_vehicleSelection.Visibility = ViewStates.Gone;
                _vehicleSubSelection.Visibility = ViewStates.Gone;
				_etaLabel.Visibility = (Eta.HasValue() && _showEta) ? ViewStates.Visible : ViewStates.Gone;
            }
            else
            {
                this.SetBackgroundColorWithRoundedCorners(0, 0, 3, 3, Color.Transparent);
				_rideEstimate.Visibility = ViewStates.Gone;

				_vehicleSelection.Visibility = ShowVehicleSelection ? ViewStates.Visible : ViewStates.Gone;
                _vehicleSelection.RemoveAllViews ();
                _vehicleSubSelection.RemoveAllViews();

                _vehicleSelectionAndEta.Visibility = ShowVehicleSelection ? ViewStates.Visible : ViewStates.Gone;

                if (ShowVehicleSelection) 
				{
					_horizontalDivider.SetBackgroundColor(Resources.GetColor(Resource.Color.orderoptions_horizontal_divider));

                    if (Vehicles.None())
                    {
                        return;
                    }

                    foreach (var vehicle in VehicleRepresentations) 
                    {
                        DrawVehicleForMainSelector(vehicle);
                    }
				}
            }

			this.Visibility = ViewStates.Visible;
        }

        private void DrawVehicleForMainSelector(VehicleType vehicle)
        {
            var currentIsSelected = IsVehicleSelected(vehicle);

            var vehicleView = new VehicleTypeControl(base.Context, vehicle, currentIsSelected);

            var layoutParameters = new LinearLayout.LayoutParams (0, ViewGroup.LayoutParams.MatchParent);
            layoutParameters.Weight = 1.0f;
            vehicleView.LayoutParameters = layoutParameters;

            vehicleView.Click += (sender, e) => { 
                if (VehicleSelected != null) 
                {
                    VehicleSelected(new OrderOptionsViewModel.VehicleSelectionModel { VehicleType = vehicle, IsSubMenuSelection = false });
                }
            };

            _vehicleSelection.AddView (vehicleView);

            if (GroupVehiclesByServiceType && currentIsSelected)
            {
                var vehiclesForThisServiceType = Vehicles.Where(x => x.ServiceType == SelectedVehicle.ServiceType).ToList();
                if (vehiclesForThisServiceType.Count > 1)
                {
                    foreach (var vehicleType in vehiclesForThisServiceType)
                    {
                        DrawVehicleForSubSelector(vehicleType);
                    }

                    _vehicleSubSelection.Visibility = ViewStates.Visible;
                }
                else
                {
                    _vehicleSubSelection.Visibility = ViewStates.Gone;
                }
            }
        }

        private void DrawVehicleForSubSelector(VehicleType vehicle)
        {
            var vehicleView = new VehicleTypeSubControl(base.Context, vehicle, IsVehicleSelected(vehicle, true));

            var layoutParameters = new LinearLayout.LayoutParams (0, ViewGroup.LayoutParams.MatchParent);
            layoutParameters.Weight = 1.0f;
            vehicleView.LayoutParameters = layoutParameters;

            vehicleView.Click += (sender, e) => { 
                if (VehicleSelected != null) 
                {
                    VehicleSelected(new OrderOptionsViewModel.VehicleSelectionModel { VehicleType = vehicle, IsSubMenuSelection = true });
                }
            };

            _vehicleSubSelection.AddView (vehicleView);
        }

        private bool IsVehicleSelected(VehicleType vehicle, bool isForSubSelection = false)
        {
            return SelectedVehicle != null && ((isForSubSelection || !GroupVehiclesByServiceType) 
                ? vehicle.Id == SelectedVehicle.Id 
                : vehicle.ServiceType == SelectedVehicle.ServiceType);
        }

        public bool IsReadOnly { get; set; }

        public VehicleType SelectedVehicle
        {
            get { return _estimateSelectedVehicleType.Vehicle; }
            set
            {
                if (_estimateSelectedVehicleType.Vehicle != value)
                {
                    _estimateSelectedVehicleType.Vehicle = value;
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
                _showEta = value;
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

        public string EstimatedFare
        {
            get{ return _estimatedFareLabel.Text; }
            set
            {
                if (_estimatedFareLabel.Text != value)
                {
                    _estimatedFareLabel.Text = value;
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

        private string _eta;
        public string Eta
        {
            get { return _eta; }
            set
            {
                if (_eta != value)
                {
                    _eta = value;
                    _etaLabel.Text = _eta;
                    Redraw ();
                }
            }
        }
    }
}

