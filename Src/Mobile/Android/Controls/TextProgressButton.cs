using System;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class TextProgressButton : LinearLayout
    {
        private static readonly int ProgressSize = DrawHelper.GetPixels(30);
        private static readonly int XPositionText = DrawHelper.GetPixels(4);
        private static readonly int YPositionTextL1 = DrawHelper.GetPixels(19);
        private static readonly int YPositionTextL2 = DrawHelper.GetPixels(36);
        private static readonly int YPositionTextOnlyL1 = DrawHelper.GetPixels(26);
        private static readonly int FontTextL1 = DrawHelper.GetPixels(20);
        private static readonly int FontTextL2 = DrawHelper.GetPixels(15);

        private ProgressBar _bar;
        private ImageView _image;
        private bool _isPlaceHolder;
        private bool _isProgressing;
        private string _textLine2;


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


        public TextProgressButton(IntPtr ptr, JniHandleOwnership handle)
            : base(ptr, handle)
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
                Invalidate();
            }
        }


// ReSharper disable UnusedAutoPropertyAccessor.Global
        public string TextLine1 { get; set; }
// ReSharper restore UnusedAutoPropertyAccessor.Global


        public string TextLine2
        {
            get { return _textLine2; }
            set
            {
                _textLine2 = value;
                Invalidate();
            }
        }

        public bool IsPlaceHolder
        {
            get { return _isPlaceHolder; }
            set
            {
                _isPlaceHolder = value;
                Invalidate();
            }
        }

        public event EventHandler Start;

        public event EventHandler Cancel;

        private void Initialize()
        {
            Touch += HandleTouch;
            Click += TextProgressButton_Click;
            SetGravity(GravityFlags.CenterVertical | GravityFlags.Right);
            Orientation = Orientation.Vertical;

            _bar = new ProgressBar(Context, null, Android.Resource.Attribute.ProgressBarStyleLarge);
            _bar.Indeterminate = true;
            _bar.Visibility = ViewStates.Gone;
            var layout = new LayoutParams(ProgressSize, ProgressSize);
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

        private void HandleTouch(object sender, TouchEventArgs e)
        {
            if (e.Event.Action == MotionEventActions.Down)
            {
                SetBackgroundColor(new Color(0, 0, 0, 50));
            }
            else if (e.Event.Action == MotionEventActions.Up)
            {
                SetBackgroundColor(new Color(0, 0, 0, 0));
            }
            e.Handled = false;
        }

        private void TextProgressButton_Click(object sender, EventArgs e)
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

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);


            DrawText(canvas, TextLine1 ?? "", XPositionText,
                TextLine2.HasValue() ? YPositionTextL1 : YPositionTextOnlyL1,
                IsProgressing || IsPlaceHolder ? FontTextL2 : FontTextL1,
                IsProgressing || IsPlaceHolder ? AppFonts.Italic : AppFonts.Regular,
                IsProgressing || IsPlaceHolder ? new Color(86, 86, 86, 255) : new Color(50, 50, 50, 255));
            DrawText(canvas, TextLine2 ?? "", XPositionText, YPositionTextL2, FontTextL2, AppFonts.Regular,
                new Color(86, 86, 86, 255));
        }


        private void DrawText(Canvas canvas, string text, float x, float y, float textSize,
            Typeface typeFace, Color color)
        {
            var wManager = (IWindowManager) Context.GetSystemService(Context.WindowService);

            var metrics = new DisplayMetrics();
            wManager.DefaultDisplay.GetMetrics(metrics);


            var paintText = new TextPaint(PaintFlags.AntiAlias | PaintFlags.LinearText);
            paintText.TextSize = textSize;
            paintText.SetARGB(color.A, color.R, color.G, color.B);
            paintText.SetTypeface(typeFace);

            var p = new TextPaint();
            p.SetTypeface(typeFace);

            p.TextSize = textSize;
            var ellipsizedText = TextUtils.Ellipsize(text, p, Width - _image.Width - 10, TextUtils.TruncateAt.End);
            if (ellipsizedText.IsNullOrEmpty())
            {
                ellipsizedText = text;
            }
            canvas.DrawText(ellipsizedText, x, y, paintText);
        }
    }
}