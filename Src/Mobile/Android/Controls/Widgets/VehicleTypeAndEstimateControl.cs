using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.Client.Localization;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Api.Contract.Resources;
using System.Collections.Generic;
using System;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public class VehicleTypeAndEstimateControl : LinearLayout
    {
        private AutoResizeTextView EstimatedFareLabel { get; set; }
		private AutoResizeTextView EtaLabel { get; set; }
		private AutoResizeTextView EtaLabelInVehicleSelection { get; set; }
        private View HorizontalDivider { get; set; }
		private LinearLayout VehicleSelectionContainer { get; set; }
		private LinearLayout VehicleSelection { get; set; }
		private LinearLayout RideEstimate { get; set; }
		private VehicleTypeControl EstimateSelectedVehicleType { get; set; }
		public Action<VehicleType> VehicleSelected { get; set; }

        public VehicleTypeAndEstimateControl(Context c, IAttributeSet attr) : base(c, attr)
        {

        }

        protected override void OnFinishInflate()
        {
            base.OnFinishInflate();
            var inflater = (LayoutInflater) Context.GetSystemService(Context.LayoutInflaterService);
            var layout = inflater.Inflate(Resource.Layout.Control_VehicleTypeAndEstimate, this, true);

			HorizontalDivider = (View)layout.FindViewById(Resource.Id.HorizontalDivider);
			RideEstimate = (LinearLayout)layout.FindViewById (Resource.Id.RideEstimate);
			VehicleSelectionContainer = (LinearLayout)layout.FindViewById (Resource.Id.VehicleSelectionContainer);
			VehicleSelection = (LinearLayout)layout.FindViewById (Resource.Id.VehicleSelection);

            EstimatedFareLabel = (AutoResizeTextView)layout.FindViewById(Resource.Id.estimateFareAutoResizeLabel);
			EtaLabel = (AutoResizeTextView)layout.FindViewById(Resource.Id.etaAutoResizeLabel);
			EtaLabelInVehicleSelection = (AutoResizeTextView)layout.FindViewById(Resource.Id.etaAutoResizeLabel);
			EstimateSelectedVehicleType = (VehicleTypeControl)layout.FindViewById (Resource.Id.estimateSelectedVehicle);
            EstimateSelectedVehicleType.Selected = true;

            this.SetBackgroundColorWithRoundedCorners(0, 0, 3, 3, Resources.GetColor(Resource.Color.company_color));
        }
			
		public bool IsReadOnly { get; set; }

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

        public string EstimatedFare
        {
            get{ return EstimatedFareLabel.Text; }
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

		bool _showVehicleSelectionContainer
		{
			get {
				return ShowEta || ShowVehicleSelection;
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
					EtaLabel.Text = _eta;
					EtaLabelInVehicleSelection.Text = _eta;
					Redraw ();
				}
			}
		}

        private void Redraw()
        {
			if (ShowEstimate)
            {
				this.SetBackgroundColorWithRoundedCorners(0, 0, 3, 3, Resources.GetColor(Resource.Color.company_color));
                HorizontalDivider.Background.SetColorFilter(Resources.GetColor(Resource.Color.company_color), PorterDuff.Mode.SrcAtop);
				RideEstimate.Visibility = ViewStates.Visible;
				VehicleSelection.Visibility = ViewStates.Gone;
            }
            else
            {
                this.SetBackgroundColorWithRoundedCorners(0, 0, 3, 3, Color.Transparent);
				RideEstimate.Visibility = ViewStates.Gone;

				VehicleSelectionContainer.Visibility = _showVehicleSelectionContainer ? ViewStates.Visible : ViewStates.Gone;
				VehicleSelection.Visibility = ShowVehicleSelection ? ViewStates.Visible : ViewStates.Gone;
				EtaLabelInVehicleSelection.Visibility = ShowEta ? ViewStates.Visible : ViewStates.Gone;

				VehicleSelection.RemoveAllViews ();

				if (_showVehicleSelectionContainer) 
				{
					HorizontalDivider.SetBackgroundColor(Resources.GetColor(Resource.Color.orderoptions_horizontal_divider));
				}

				if (ShowVehicleSelection) {

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

						VehicleSelection.AddView (vehicleView);
					}
				}
            }
        }
    }
}

