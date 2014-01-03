using Android.Content;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Slider
{
    public class ProgressBarControl : Control
    {
        private const int ControlHeight = 60;
        private readonly ImageView _barBody;
        private readonly ImageView _barEnd;

        private readonly int _barHeight;
        private readonly ImageView _barStart;

        public ProgressBarControl(Context c, int barStartImageId, int barBodyImageId, int barEndImageId, int width)
            : base(c, width, ControlHeight)
        {
            _barStart = new ImageView(c);
            _barEnd = new ImageView(c);
            _barBody = new ImageView(c);

            if (barStartImageId != default(int))
            {
                _barStart.SetImageResource(barStartImageId);
                _barStart.SetSize(_barStart.GetBitmapSize());

                AddViews(_barStart);
            }

            if (barEndImageId != default(int))
            {
                _barEnd.SetImageResource(barEndImageId);
                _barEnd.SetSize(_barEnd.GetBitmapSize());
                _barEnd.AddLayoutRule(LayoutRules.AlignParentRight);
                AddViews(_barEnd);
            }

            _barBody.SetBackgroundResource(barBodyImageId);

            var leftNubWidth = _barStart.GetSize().Width;

            var barWidth = width - leftNubWidth - _barEnd.GetSize().Width;
            var barHeight = _barBody.GetBackgroundBitmapSize().Height;

            _barBody.SetSize(barWidth, barHeight);
            _barBody.SetPosition(leftNubWidth, 0);

            AddViews(_barBody);

            var barEndSize = _barEnd.GetSize();
            _barHeight = barEndSize.Height;
        }

        public void Resize(int endPosition)
        {
            _barEnd.SetWidth(endPosition);
            _barBody.SetFrame(_barStart.GetRight(), _barStart.GetTop(), _barEnd.GetLeft() - _barStart.GetRight(),
                _barHeight);
        }
    }
}