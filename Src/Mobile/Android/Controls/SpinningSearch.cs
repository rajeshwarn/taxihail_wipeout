using System;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Helpers;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class SpinningSearch : LinearLayout
    {
        private static readonly int ProgressSize = DrawHelper.GetPixels(28);
        private static readonly int SearchImageSearch = DrawHelper.GetPixels(20);


        private ProgressBar _bar;
        private ImageView _image;
        private bool _isProgressing;


        public SpinningSearch(Context context) : base(context)
        {
            Initialize();
        }

        public SpinningSearch(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Initialize();
        }

        public SpinningSearch(IntPtr ptr, JniHandleOwnership handle) : base(ptr, handle)
        {
            Initialize();
        }


        public bool IsProgressing
        {
            get { return _isProgressing; }
            set
            {
                _isProgressing = value;
                if (IsProgressing)
                {
                    _image.Visibility = ViewStates.Gone;
                    _bar.Visibility = ViewStates.Visible;
                }
                else
                {
                    _image.Visibility = ViewStates.Visible;
                    _bar.Visibility = ViewStates.Gone;
                }
            }
        }

        private void Initialize()
        {
            SetBackgroundColor(Color.Transparent);
            SetGravity(GravityFlags.CenterVertical | GravityFlags.CenterHorizontal);
            Orientation = Orientation.Horizontal;

            _bar = new ProgressBar(Context, null, Android.Resource.Attribute.ProgressBarStyleLarge);
            _bar.Indeterminate = true;
            _bar.Visibility = ViewStates.Gone;
            _bar.IndeterminateDrawable.SetColorFilter(Color.Rgb(147, 152, 157), PorterDuff.Mode.Multiply);
            var layout = new LayoutParams(ProgressSize, ProgressSize);
            layout.Gravity = GravityFlags.CenterVertical | GravityFlags.Left;
            AddView(_bar, layout);


            _image = new ImageView(Context, null);
            _image.SetScaleType(ImageView.ScaleType.Center);

            _image.Visibility = ViewStates.Visible;
            layout = new LayoutParams(SearchImageSearch, SearchImageSearch);
            layout.Gravity = GravityFlags.CenterVertical | GravityFlags.Left;
            AddView(_image, layout);

            _image.SetImageDrawable(Context.Resources.GetDrawable(Resource.Drawable.loupe));
        }
    }
}