using Android.Widget;
using Android.Content;
using Android.Util;
using Android.Views;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;
using Android.Graphics;
using apcurium.MK.Booking.Api.Contract.Resources;
using Android.Runtime;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("apcurium.mk.booking.mobile.client.controls.widgets.VehicleTypeSubControl")]
    public class VehicleTypeSubControl : LinearLayout
    {
        private TextView VehicleTypeLabel { get; set; }

        private Color DefaultColorForText { get { return Resources.GetColor(Resource.Color.gray); } }

        public VehicleTypeSubControl(Context c, IAttributeSet attr) : base(c, attr)
        {
        }

        public VehicleTypeSubControl(Context c, VehicleType vehicle, bool isSelected) : base(c)
        {
            Initialize ();

            Vehicle = vehicle;
            Selected = isSelected;
            Clickable = true;
        }

        protected override void OnFinishInflate()
        {
            base.OnFinishInflate();

            Initialize ();
        }

        private void Initialize()
        {
            var inflater = (LayoutInflater) Context.GetSystemService(Context.LayoutInflaterService);
            var layout = inflater.Inflate(Resource.Layout.Control_VehicleTypeSub, this, true);

            VehicleTypeLabel = (TextView)layout.FindViewById(Resource.Id.vehicleTypeSubLabel);
        }

        private VehicleType _vehicle;
        public VehicleType Vehicle
        {
            get { return _vehicle; }
            set
            {
                if (_vehicle != value)
                {
                    _vehicle = value;

                    VehicleTypeLabel.Text = TinyIoCContainer.Current.Resolve<ILocalization>()[value.Name].ToUpper();
                    VehicleTypeLabel.SetTextColor (DefaultColorForText);
                }
            }
        }

        public override bool Selected 
        {
            get { return base.Selected; }
            set 
            {
                if (base.Selected != value)
                {
                    base.Selected = value;

                    if (Vehicle == null)
                    {
                        return;
                    }

                    if (value) 
                    {
                        VehicleTypeLabel.SetTextColor (Resources.GetColor(Resource.Color.company_color));
                    } 
                    else 
                    {
                        VehicleTypeLabel.SetTextColor (DefaultColorForText);
                    }
                }
            }
        }
    }
}
