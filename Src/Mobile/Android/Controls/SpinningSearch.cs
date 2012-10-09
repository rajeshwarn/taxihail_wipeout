using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;
using Android.Graphics;
using Android.Text;
using apcurium.MK.Booking.Mobile.Client.Helpers;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
	public class SpinningSearch : LinearLayout
    {
        private static int _progressSize = DrawHelper.GetPixels(28);
        private static int _searchImageSearch = DrawHelper.GetPixels(20);
        

        private bool _isProgressing;
        private ProgressBar _bar;
        private ImageView _image;

        

		public SpinningSearch(Context context) : base(context)
        {
            Initialize();
        }

		public SpinningSearch(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Initialize();              
        }

		public SpinningSearch(IntPtr ptr, Android.Runtime.JniHandleOwnership handle) : base(ptr, handle)
        {
            Initialize();
        }

        private void Initialize()
        {
			this.SetBackgroundColor( Color.Transparent );
			this.SetGravity(GravityFlags.CenterVertical | GravityFlags.CenterHorizontal);
			this.Orientation = Android.Widget.Orientation.Horizontal;

            _bar = new ProgressBar(Context, null, Android.Resource.Attribute.ProgressBarStyleLarge);
            _bar.Indeterminate = true;
            _bar.Visibility = ViewStates.Gone;
			_bar.IndeterminateDrawable.SetColorFilter( Color.Rgb(147,152,157), PorterDuff.Mode.Multiply );
            var layout = new LinearLayout.LayoutParams(_progressSize, _progressSize);
            layout.Gravity = GravityFlags.CenterVertical | GravityFlags.Left;
            AddView(_bar, layout);

            
			_image= new ImageView( Context, null);
            _image.SetScaleType(ImageView.ScaleType.Center);
            
            _image.Visibility = ViewStates.Visible;
            layout = new LayoutParams(_searchImageSearch, _searchImageSearch);
            layout.Gravity = GravityFlags.CenterVertical | GravityFlags.Left;
            AddView(_image, layout);

            _image.SetImageDrawable(Context.Resources.GetDrawable(Resource.Drawable.loupe));  
        }


        public bool IsProgressing 
        { 
            get
            {
                return _isProgressing;
            }
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
                
    }

}