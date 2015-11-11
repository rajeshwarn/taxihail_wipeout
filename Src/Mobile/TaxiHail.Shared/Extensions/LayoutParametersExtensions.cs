using Android.Views;
using Android.Widget;

namespace apcurium.MK.Booking.Mobile.Client.Extensions
{
    public static class LayoutParametersExtensions
    {
        public static RelativeLayout.LayoutParams ToRelative(this ViewGroup.LayoutParams thisLayoutParam)
        {
            return new RelativeLayout.LayoutParams(thisLayoutParam.Width, thisLayoutParam.Height);
        }

        public static TableLayout.LayoutParams ToTable(this ViewGroup.LayoutParams thisLayoutParam)
        {
            return new TableLayout.LayoutParams(thisLayoutParam.Width, thisLayoutParam.Height);
        }

        public static TableRow.LayoutParams ToTableRow(this ViewGroup.LayoutParams thisLayoutParam)
        {
            return new TableRow.LayoutParams(thisLayoutParam.Width, thisLayoutParam.Height);
        }

        public static RelativeLayout.LayoutParams AsRelative(this ViewGroup.LayoutParams thisLayoutParam)
        {
            if (thisLayoutParam == null)
            {
                return new RelativeLayout.LayoutParams(0, 0);
            }
            if (thisLayoutParam is RelativeLayout.LayoutParams)
            {
                return (RelativeLayout.LayoutParams) thisLayoutParam;
            }

            return thisLayoutParam.ToRelative();
        }
    }
}