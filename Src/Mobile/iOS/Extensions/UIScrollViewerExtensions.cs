using MonoTouch.UIKit;
using System.Drawing;
using System.Linq;

namespace apcurium.MK.Booking.Mobile.Client.Extensions
{
    public static class UIScrollViewerExtensions
    {
        public static void AutoSize(this UIScrollView thisScrollViewer){
            
            var maxWidth = 0f;
            var maxHeight = 0f;
            foreach (var view in thisScrollViewer.Subviews.Where(v=>!v.Hidden)) 
            {   
                if(view.Frame.Bottom > maxHeight)
                {
                    maxHeight = view.Frame.Bottom;
                }
                
                var farRight = view.Frame.Width+view.Frame.X;
                if(farRight > maxWidth)
                {
                    maxWidth = farRight;
                }
            }
            
            thisScrollViewer.ContentSize = new SizeF (maxWidth, maxHeight);
        }
        
        public static void DisableHorizontalScroll(this UIScrollView thisScrollViewer){
            thisScrollViewer.ContentSize = new SizeF (thisScrollViewer.Frame.Width, thisScrollViewer.ContentSize.Height);
        }
    }
}

