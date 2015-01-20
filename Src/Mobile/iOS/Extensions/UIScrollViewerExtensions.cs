using UIKit;
using CoreGraphics;
using System.Linq;
using System;

namespace apcurium.MK.Booking.Mobile.Client.Extensions
{
    public static class UIScrollViewerExtensions
    {
        public static void AutoSize(this UIScrollView thisScrollViewer){
            
            nfloat maxWidth = 0f;
            nfloat maxHeight = 0f;
            foreach (var view in thisScrollViewer.Subviews.Where(v=>!v.Hidden)) 
            {   
                if(view.Frame.Bottom > maxHeight)
                {
                    maxHeight = view.Frame.Bottom;
                }
                
                var farRight = view.Frame.Width + view.Frame.X;
                if(farRight > maxWidth)
                {
                    maxWidth = farRight;
                }
            }
            
            thisScrollViewer.ContentSize = new CGSize (maxWidth, maxHeight);
        }
        
        public static void DisableHorizontalScroll(this UIScrollView thisScrollViewer){
            thisScrollViewer.ContentSize = new CGSize (thisScrollViewer.Frame.Width, thisScrollViewer.ContentSize.Height);
        }
    }
}

