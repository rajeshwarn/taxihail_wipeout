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

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class TextProgressButton : LinearLayout
    {
        private static int _rightArrowDrawableId;
        private bool _isProgressing;
        private ProgressBar _bar;
        private ImageView _image;

        public event EventHandler Start;
        
        public event EventHandler Cancel;

        
        public TextProgressButton(Context context)
            : base(context)
        {
            Initialize();
        }

        public TextProgressButton(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            Initialize();              


           
        }

       
        public TextProgressButton(IntPtr ptr, Android.Runtime.JniHandleOwnership handle)
            : base(ptr, handle)
        {
            Initialize();
        }

        private void Initialize()
        {
            this.Click += new EventHandler(TextProgressButton_Click);
            this.SetGravity(GravityFlags.CenterVertical | GravityFlags.Right);
            this.Orientation = Android.Widget.Orientation.Vertical;

            _bar = new ProgressBar(Context, null, Android.Resource.Attribute.ProgressBarStyleLarge);
            _bar.Indeterminate = true;
            _bar.Visibility = ViewStates.Gone;
            var layout = new LinearLayout.LayoutParams(40, 40);
            layout.Gravity = GravityFlags.CenterVertical | GravityFlags.Right;
            AddView(_bar, layout);

            _image= new ImageView( Context, null);
            _image.SetScaleType(ImageView.ScaleType.Center);
            
            _image.Visibility = ViewStates.Visible;
            layout = new LayoutParams(40, 40);
            layout.Gravity = GravityFlags.CenterVertical | GravityFlags.Right;
            AddView(_image, layout);

            if (_rightArrowDrawableId == 0)
            {
                _rightArrowDrawableId = Context.Resources.GetIdentifier("right_arrow", "drawable", Context.PackageName);
            }
            _image.SetImageDrawable(Context.Resources.GetDrawable(_rightArrowDrawableId));
            ///android:drawableRight="@drawable/right_arrow"
            
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

        
        

        void TextProgressButton_Click(object sender, EventArgs e)
        {
            if (IsProgressing)
            {
                if (Cancel != null)
                {
                    Cancel(this, EventArgs.Empty);
                }
            }
            else
            {
                if (Start != null)
                {
                    Start(this, EventArgs.Empty);
                }
            }
        }


        public string TextLine1 { get; set; }

        public string TextLine2 { get; set; }

        protected override void OnAttachedToWindow()
        {
         

            base.OnAttachedToWindow();

          
        }
        protected override void OnDraw(Android.Graphics.Canvas canvas)
        {
            base.OnDraw(canvas);


            DrawText(canvas, TextLine1 ?? "", 8, 20, 15);
            DrawText(canvas, TextLine2 ?? "", 8, 45, 20);


        }

        private void DrawText(Android.Graphics.Canvas canvas, string text, float x, float y, float textSize)
        {
            TextPaint paintText = new TextPaint(PaintFlags.AntiAlias | Android.Graphics.PaintFlags.LinearText);
            var rect = new Rect();
            paintText.TextSize = textSize;
            paintText.GetTextBounds(text, 0, text.Length, rect);
            paintText.SetARGB(255, 49, 49, 49);
            paintText.SetTypeface(AppFonts.Regular);
            canvas.DrawText(text, x, y, paintText);

        }
                
    }
    public class TextButton : Button
    {
        public TextButton(Context context)
            : base(context)
        {
        }

        public TextButton(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
        }

        public TextButton(IntPtr ptr, Android.Runtime.JniHandleOwnership handle)
            : base(ptr, handle)
        {


        }



        public string TextLine1 { get; set; }

        public string TextLine2 { get; set; }

        protected override void OnDraw(Android.Graphics.Canvas canvas)
        {
            base.OnDraw(canvas);


            DrawText(canvas, TextLine1 ?? "", 8, 20, 15);
            DrawText(canvas, TextLine2 ?? "", 8, 45, 20);


        }

        private void DrawText(Android.Graphics.Canvas canvas, string text, float x, float y, float textSize)
        {
            TextPaint paintText = new TextPaint(PaintFlags.AntiAlias | Android.Graphics.PaintFlags.LinearText);
            var rect = new Rect();
            paintText.TextSize = textSize;
            paintText.GetTextBounds(text, 0, text.Length, rect);
            paintText.SetARGB(255, 49, 49, 49);
            paintText.SetTypeface(AppFonts.Regular);
            canvas.DrawText(text, x, y, paintText);

        }

    }
}