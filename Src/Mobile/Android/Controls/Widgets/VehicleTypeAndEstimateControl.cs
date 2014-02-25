using System;
using Android.Widget;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Graphics;
using apcurium.MK.Booking.Mobile.Client.Helpers;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public class VehicleTypeAndEstimateControl : LinearLayout
    {
        private ImageView SelectedVehicleType { get; set; }
        private TextView SelectedVehicleTypeLabel { get; set; }
        private TextView EstimatedFareLabel { get; set; }
        private View HorizontalDivider { get; set; }

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
            HorizontalDivider = (View)layout.FindViewById(Resource.Id.HorizontalDivider);
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
                    _vehicleType = value;
                    var bitmap = BitmapFactory.DecodeResource(Context.Resources, Resource.Drawable.taxi_badge_selected);
                    var iconWithNewColor = DrawHelper.Colorize(bitmap, Resources.GetColor(Resource.Color.company_color));
                    SelectedVehicleType.SetImageBitmap(iconWithNewColor);
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
                HorizontalDivider.Background.SetColorFilter(Resources.GetColor(Resource.Color.company_color), PorterDuff.Mode.SrcAtop);
                SelectedVehicleType.Visibility = ViewStates.Visible;
                SelectedVehicleTypeLabel.Visibility = ViewStates.Visible;
                EstimatedFareLabel.Visibility = ViewStates.Visible;
            }
            else
            {
                HorizontalDivider.Background.SetColorFilter(Resources.GetColor(Resource.Color.orderoptions_horizontal_divider), PorterDuff.Mode.SrcAtop);
                SelectedVehicleType.Visibility = ViewStates.Gone;
                SelectedVehicleTypeLabel.Visibility = ViewStates.Gone;
                EstimatedFareLabel.Visibility = ViewStates.Gone;

                // show the buttons to select vehicle type
            }
        }
    }
}

