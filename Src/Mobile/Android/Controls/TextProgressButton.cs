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
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Mobile.Client.Helpers;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class TextProgressButton : LinearLayout
    {
        private static int _progressSize = DrawHelper.GetPixels(30);
        private static int _xPositionText = DrawHelper.GetPixels(4);
        private static int _yPositionTextL1 = DrawHelper.GetPixels(19);
        private static int _yPositionTextL2 = DrawHelper.GetPixels(38);
        private static int _yPositionTextOnlyL1 = DrawHelper.GetPixels(26);
        private static int _fontTextL1 = DrawHelper.GetPixels(20);
        private static int _fontTextL2 = DrawHelper.GetPixels(15);

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
			this.Touch += HandleTouch;
            this.Click += new EventHandler(TextProgressButton_Click);
            this.SetGravity(GravityFlags.CenterVertical | GravityFlags.Right);
            this.Orientation = Android.Widget.Orientation.Vertical;

            _bar = new ProgressBar(Context, null, Android.Resource.Attribute.ProgressBarStyleLarge);
            _bar.Indeterminate = true;
            _bar.Visibility = ViewStates.Gone;
            var layout = new LinearLayout.LayoutParams(_progressSize, _progressSize);
            layout.Gravity = GravityFlags.CenterVertical | GravityFlags.Right;
            AddView(_bar, layout);

            _image = new ImageView(Context, null);
            _image.SetScaleType(ImageView.ScaleType.Center);

            _image.Visibility = ViewStates.Visible;
            layout = new LayoutParams(40, 40);
            layout.Gravity = GravityFlags.CenterVertical | GravityFlags.Right;
            AddView(_image, layout);

			_image.SetImageDrawable(Context.Resources.GetDrawable(Resource.Drawable.right_arrow));

        }

        void HandleTouch (object sender, TouchEventArgs e)
		{
			if (e.Event.Action == MotionEventActions.Down) {
				this.SetBackgroundColor(new Color(0,0,0,50));
					
			} else if (e.Event.Action == MotionEventActions.Up) {
				this.SetBackgroundColor(new Color(0,0,0,0));
			}
			e.Handled=false;
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
                Invalidate();
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


        private string _textLine2;

        public string TextLine2
        {
            get { return _textLine2; }
            set
            {
                _textLine2 = value;
                Invalidate();
            }
        }

        private bool _isPlaceHolder;

        public bool IsPlaceHolder
        {
            get { return _isPlaceHolder; }
            set
            {
                _isPlaceHolder = value;
                Invalidate();
            }
        }


        protected override void OnAttachedToWindow()
        {


            base.OnAttachedToWindow();


        }
        protected override void OnDraw(Android.Graphics.Canvas canvas)
        {
            base.OnDraw(canvas);


            DrawText(canvas, TextLine1 ?? "", _xPositionText, TextLine2.HasValue() ? _yPositionTextL1 : _yPositionTextOnlyL1, IsProgressing || IsPlaceHolder ? _fontTextL2 : _fontTextL1 , IsProgressing || IsPlaceHolder ? AppFonts.Italic : AppFonts.Regular, IsProgressing || IsPlaceHolder ? new Color( 86,86,86,255 ) : new Color( 50,50,50,255 ) );
            DrawText(canvas, TextLine2 ?? "", _xPositionText, _yPositionTextL2, _fontTextL2, AppFonts.Regular, new Color( 86,86,86,255 ));


        }

   

        private void DrawText(Android.Graphics.Canvas canvas, string text, float x, float y, float textSize, Typeface typeFace, Color color)
        {
            var wManager = (IWindowManager)Context.GetSystemService(Context.WindowService);

            var metrics = new DisplayMetrics();
            wManager.DefaultDisplay.GetMetrics(metrics);

            


            TextPaint paintText = new TextPaint(PaintFlags.AntiAlias | Android.Graphics.PaintFlags.LinearText);
            paintText.TextSize = textSize;
            paintText.SetARGB(color.A, color.R  , color.G , color.B );
            paintText.SetTypeface(typeFace);

            var p = new TextPaint();
            p.SetTypeface(typeFace);

            p.TextSize = textSize;
            var ellipsizedText = TextUtils.Ellipsize(text, p, this.Width - _image.Width - 10, TextUtils.TruncateAt.End);
            if (ellipsizedText.IsNullOrEmpty())
            {
                ellipsizedText = text;
            }
            canvas.DrawText(ellipsizedText, x, y, paintText);

        }

    }
}

