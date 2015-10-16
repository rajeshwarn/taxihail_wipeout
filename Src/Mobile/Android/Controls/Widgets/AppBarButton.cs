using Android.Widget;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Runtime;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("apcurium.mk.booking.mobile.client.controls.widgets.AppBarButton")]
    public class AppBarButton : LinearLayout
    {
        private ImageView ButtonImage { get; set; }
        private TextView ButtonLabel { get; set; }
        private IAttributeSet _attr;

        public AppBarButton(Context c, IAttributeSet attr) : base(c, attr)
        {
            _attr = attr;
        }

        protected override void OnFinishInflate()
        {
            base.OnFinishInflate();

            Initialize ();
        }

        private int GetImageResourceId()
        {
            var att = Context.Theme.ObtainStyledAttributes(_attr, Resource.Styleable.AppBarButton, 0, 0);
            return att.GetResourceId(Resource.Styleable.AppBarButton_ImgSrc, 0);
        }

        private int GetString()
        {
            var att = Context.Theme.ObtainStyledAttributes(_attr, Resource.Styleable.AppBarButton, 0, 0);
            return att.GetResourceId(Resource.Styleable.AppBarButton_LabelText, 0);
        }

        private void Initialize()
        {
            var inflater = (LayoutInflater) Context.GetSystemService(Context.LayoutInflaterService);
            var layout = inflater.Inflate(Resource.Layout.Control_AppBarButton, this, true);
        
            var test1 = GetImageResourceId();
            var test2 = GetString();

            ButtonImage = (ImageView)layout.FindViewById(Resource.Id.buttonImage);
            ButtonImage.SetBackgroundResource(Resource.Drawable.button_booklater_image_selector);
            ButtonLabel = (TextView)layout.FindViewById(Resource.Id.buttonLabel);
            ButtonLabel.Text = Context.GetString(Resource.String.BookItLaterButton);
        }

        public override bool Enabled
        {
            get { return base.Enabled; }
            set
            {
                base.Enabled = value;
                base.Alpha = Enabled
                    ? 1.0f
                    : 0.5f;

                if (ButtonImage != null && ButtonLabel != null)
                {
                    ButtonImage.Alpha = ButtonLabel.Alpha = base.Alpha;
                }
            }
        }

        public override bool Selected
        {
            get { return base.Selected; }
            set
            {
                base.Selected = value;
                ButtonImage.Selected = value;
                ButtonLabel.Selected = value;
            }
        }
    }
}
