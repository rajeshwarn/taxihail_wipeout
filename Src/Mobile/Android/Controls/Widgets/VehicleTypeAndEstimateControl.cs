using System;
using Android.Widget;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Graphics;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public class VehicleTypeAndEstimateControl : LinearLayout
    {
        private ImageView SelectedVehicleType { get; set; }
        private TextView SelectedVehicleTypeLabel { get; set; }
        private TextView EstimatedFareLabel { get; set; }

        private Color Blue = Color.Rgb(0, 129, 248);

        public VehicleTypeAndEstimateControl(Context c, IAttributeSet attr) : base(c, attr)
        {

        }

        protected override void OnFinishInflate()
        {
            base.OnFinishInflate();
            var inflater = (LayoutInflater) Context.GetSystemService(Context.LayoutInflaterService);
            var layout = inflater.Inflate(Resource.Layout.Control_VehicleTypeAndEstimate, this, true);

            SelectedVehicleType = (ImageView)layout.FindViewById(Resource.Id.vehicleTypeImage);
            SelectedVehicleTypeLabel = (TextView)layout.FindViewById(Resource.Id.vehicleTypeLabel);
            EstimatedFareLabel = (TextView)layout.FindViewById(Resource.Id.estimateFareLabel);
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
                    SelectedVehicleType.SetImageDrawable(Resources.GetDrawable(Resource.Drawable.taxi_badge_selected));
                    SelectedVehicleTypeLabel.Text = value.ToUpper();
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
//                BackgroundColor = UIColor.FromRGB(230, 230, 230).ColorWithAlpha(0.5f);
//                HorizontalDividerTop.BackgroundColor = Blue;
                SelectedVehicleType.Visibility = ViewStates.Visible;
                SelectedVehicleTypeLabel.Visibility = ViewStates.Visible;
                EstimatedFareLabel.Visibility = ViewStates.Visible;
            }
            else
            {
//                BackgroundColor = UIColor.Clear;
//                HorizontalDividerTop.BackgroundColor = UIColor.FromRGB(177, 177, 177);
                SelectedVehicleType.Visibility = ViewStates.Gone;
                SelectedVehicleTypeLabel.Visibility = ViewStates.Gone;
                EstimatedFareLabel.Visibility = ViewStates.Gone;

                // show the buttons to select vehicle type
            }
        }
    }
}

