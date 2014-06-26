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

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public class VehicleTypeAndEstimateControl : LinearLayout
    {
        private TextView EstimatedFareLabel { get; set; }
        private View HorizontalDivider { get; set; }
		private LinearLayout VehicleSelection { get; set; }
		private LinearLayout RideEstimate { get; set; }
		private VehicleType EstimateSelectedVehicleType { get; set; }

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
			VehicleSelection = (LinearLayout)layout.FindViewById (Resource.Id.VehicleSelection);

			EstimatedFareLabel = (TextView)layout.FindViewById(Resource.Id.estimateFareLabel);
			EstimateSelectedVehicleType = (VehicleType)layout.FindViewById (Resource.Id.estimateSelectedVehicle);

            this.SetBackgroundColorWithRoundedCorners(0, 0, 3, 3, Resources.GetColor(Resource.Color.company_color));
        }
			
		public apcurium.MK.Booking.Api.Contract.Resources.VehicleType SelectedVehicle
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

		private apcurium.MK.Booking.Api.Contract.Resources.VehicleType[] _vehicles;
		public apcurium.MK.Booking.Api.Contract.Resources.VehicleType[] Vehicles
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
            get { return EstimatedFareLabel.Text; }
            set
            {
                if (EstimatedFareLabel.Text != value)
                {
                    EstimatedFareLabel.Text = value;
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
				this.SetBackgroundColorWithRoundedCorners(0, 0, 3, 3, Resources.GetColor(Resource.Color.white));
				HorizontalDivider.SetBackgroundColor(Resources.GetColor(Resource.Color.orderoptions_horizontal_divider));
				RideEstimate.Visibility = ViewStates.Gone;
				VehicleSelection.Visibility = ViewStates.Visible;

				VehicleSelection.RemoveAllViews ();
				foreach (var vehicle in Vehicles) 
				{
					VehicleSelection.AddView (new VehicleType (base.Context, vehicle, vehicle.Id == SelectedVehicle.Id));
				}
            }
        }
    }
}

