using System.Drawing;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Helpers;

namespace apcurium.MK.Booking.Mobile.Client.Extensions
{
    public static class ViewExtensions
    {
        public static void SetFillParent(this View thisView)
        {
            thisView.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent,
                ViewGroup.LayoutParams.FillParent);
        }

        public static void SetWrapContent(this View thisView)
        {
            thisView.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent,
                ViewGroup.LayoutParams.WrapContent);
        }

        public static void SetSize(this View thisView, int width, int height)
        {
            thisView.LayoutParameters = new ViewGroup.LayoutParams(width, height);
        }

        public static void SetSize(this View thisView, Size size)
        {
            thisView.SetSize(size.Width, size.Height);
        }

        public static Size GetSize(this View thisView)
        {
            if (thisView.LayoutParameters == null)
            {
                return new Size(0, 0);
            }

            return new Size(thisView.LayoutParameters.Width, thisView.LayoutParameters.Height);
        }

        public static Rectangle GetFrame(this View thisView)
        {
            RelativeLayout.LayoutParams layout = thisView.LayoutParameters.AsRelative();

            return new Rectangle(layout.LeftMargin, layout.TopMargin, layout.Width, layout.Height);
        }

        public static void SetFrame(this View thisView, Rectangle rec)
        {
            thisView.SetFrame(rec.X, rec.Y, rec.Width, rec.Height);
        }


        public static void SetFrame(this View thisView, int x, int y, int width, int height)
        {
            var layout = thisView.LayoutParameters.AsRelative();
            layout.Width = width;
            thisView.SetPosition(x, y);
        }

        public static void AddLayoutRule(this View thisView, params LayoutRules[] verbs)
        {
            var layout = thisView.LayoutParameters.AsRelative();

            foreach (var verb in verbs)
            {
                layout.AddRule(verb);
            }

            thisView.LayoutParameters = layout;
        }


        public static T SetPosition<T>(this T thisView, int x, int y) where T : View
        {
            var newLayout = thisView.LayoutParameters.ToRelative();
            newLayout.LeftMargin = x;
            newLayout.TopMargin = y;

            thisView.LayoutParameters = newLayout;

            return thisView;
        }

        public static T SetX<T>(this T thisView, int x) where T : View
        {
            var newLayout = thisView.LayoutParameters.ToRelative();
            newLayout.LeftMargin = x;
            thisView.LayoutParameters = newLayout;
            return thisView;
        }

        public static T SetWidth<T>(this T thisView, int width) where T : View
        {
            if (thisView.LayoutParameters == null)
            {
                thisView.LayoutParameters = new ViewGroup.LayoutParams(0, 0);
            }
            thisView.LayoutParameters.Width = width;
            return thisView;
        }

        public static int GetWidth(this View thisView)
        {
            var layout = thisView.LayoutParameters.AsRelative();
            return layout.Width;
        }

        public static int GetTop(this View thisView)
        {
            var layout = thisView.LayoutParameters.AsRelative();
            return layout.TopMargin;
        }

        public static int GetLeft(this View thisView)
        {
            var layout = thisView.LayoutParameters.AsRelative();
            return layout.LeftMargin;
        }

        public static int GetRight(this View thisView)
        {
            var layout = thisView.LayoutParameters.AsRelative();
            return layout.LeftMargin + layout.Width;
        }

        public static T SetY<T>(this T thisView, int y) where T : View
        {
            var newLayout = thisView.LayoutParameters.ToRelative();
            newLayout.TopMargin = y;
            thisView.LayoutParameters = newLayout;
            return thisView;
        }

        public static T IncrementX<T>(this T thisView, int x) where T : View
        {
            var newLayout = thisView.LayoutParameters.ToRelative();
            newLayout.LeftMargin += x;
            thisView.LayoutParameters = newLayout;
            return thisView;
        }

        public static T IncrementY<T>(this T thisView, int y) where T : View
        {
            var newLayout = thisView.LayoutParameters.ToRelative();
            newLayout.TopMargin += y;
            thisView.LayoutParameters = newLayout;
            return thisView;
        }

        public static void SetBackgroundColorWithRoundedCorners(this View view, float topLeftRadius, float topRightRadius, float bottomLeftRadius, float bottomRightRadius, Android.Graphics.Color backgroundColor)
        {            
            var sd = new GradientDrawable();
            sd.SetColor(backgroundColor);
            sd.SetCornerRadii(new float[] { topLeftRadius.ToPixels(), topLeftRadius.ToPixels(), 
                                            topRightRadius.ToPixels(), topRightRadius.ToPixels(), 
                                            bottomLeftRadius.ToPixels(), bottomLeftRadius.ToPixels(), 
                                            bottomRightRadius.ToPixels(), bottomRightRadius.ToPixels() 
            });
            view.SetBackgroundDrawable(sd);
        }
    }
}