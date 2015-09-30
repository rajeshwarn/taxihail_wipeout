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
using Android.Gms.Analytics;

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
		private LinearLayout _rideEstimate;
		private VehicleTypeControl _estimateSelectedVehicleType;

		public Action<VehicleType> VehicleSelected { get; set; }

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

            _estimatedFareLabel = (AutoResizeTextView)layout.FindViewById(Resource.Id.estimateFareAutoResizeLabel);
			
			_etaLabel = (AutoResizeTextView)layout.FindViewById(Resource.Id.EtaLabel);

			_estimateSelectedVehicleType = (VehicleTypeControl)layout.FindViewById (Resource.Id.estimateSelectedVehicle);
            _estimateSelectedVehicleType.Selected = true;

            this.SetBackgroundColorWithRoundedCorners(0, 0, 3, 3, Resources.GetColor(Resource.Color.company_color));
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

	    public bool ShowEta
	    {
		    get { return _showEta; }
		    set
		    {
			    _showEta = value;

			    _etaLabel.Visibility = _showEta ? ViewStates.Visible : ViewStates.Gone;
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

		bool _showVehicleSelectionContainer
		{
			get { return ShowVehicleSelection; }
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
	    private bool _showEta;

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

        private void Redraw()
        {
			if (ShowEstimate)
            {
				this.SetBackgroundColorWithRoundedCorners(0, 0, 3, 3, Resources.GetColor(Resource.Color.company_color));
                _horizontalDivider.Background.SetColorFilter(Resources.GetColor(Resource.Color.company_color), PorterDuff.Mode.SrcAtop);
				_rideEstimate.Visibility = ViewStates.Visible;
				_vehicleSelection.Visibility = ViewStates.Gone;
                _etaLabel.Visibility = Eta.HasValue() ? ViewStates.Visible : ViewStates.Gone;
            }
            else
            {
                this.SetBackgroundColorWithRoundedCorners(0, 0, 3, 3, Color.Transparent);
				_rideEstimate.Visibility = ViewStates.Gone;

				_vehicleSelection.Visibility = ShowVehicleSelection ? ViewStates.Visible : ViewStates.Gone;

				_vehicleSelectionAndEta.Visibility = _showVehicleSelectionContainer ? ViewStates.Visible : ViewStates.Gone;

				_vehicleSelection.RemoveAllViews ();

				if (_showVehicleSelectionContainer) 
				{
					_horizontalDivider.SetBackgroundColor(Resources.GetColor(Resource.Color.orderoptions_horizontal_divider));
				}

				if (ShowVehicleSelection && Vehicles != null) {

					foreach (var vehicle in Vehicles) {
						var vehicleView = new VehicleTypeControl (base.Context, vehicle, SelectedVehicle == null ? false : vehicle.Id == SelectedVehicle.Id);

						var layoutParameters = new LinearLayout.LayoutParams (0, LayoutParams.FillParent);
						layoutParameters.Weight = 1.0f;
						vehicleView.LayoutParameters = layoutParameters;

						vehicleView.Click += (sender, e) => { 
							if (!IsReadOnly && VehicleSelected != null) {
								VehicleSelected (vehicle);
							}
						};

						_vehicleSelection.AddView (vehicleView);
					}
				}
            }
			this.Visibility = ViewStates.Visible;
        }
    }
}

