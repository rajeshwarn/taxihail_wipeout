using System;
using Android.Graphics.Drawables;
using Android.Content.Res;

namespace apcurium.MK.Booking.Mobile.Client.Helpers
{
    public static class DrawableHelper
    {
        public static Drawable GetDrawableFromString(Resources resources, string resourceKey)
        {
            try
            {
                // finding a resource id by reflection is 5x quicker than with getIdentifier()
                // http://daniel-codes.blogspot.ca/2009/12/dynamically-retrieving-resources-in.html
                var drawableType = typeof(Resource.Drawable);
                var field = drawableType.GetField(resourceKey);
                var drawableId =  (int)field.GetValue(null);

                return resources.GetDrawable(drawableId);
            }
            catch
            {
                return null;
            }
        }
    }
}

