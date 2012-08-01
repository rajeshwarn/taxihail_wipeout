using System;
using Android.Views;
using Android.Views.Animations;

namespace apcurium.MK.Booking.Mobile.Client.Animations
{
    public class ResizeAnimation : Animation
    {
        private int OriginalHeight { get; set; }
        private int TargetHeight { get; set; }
        int OffsetHeight { get; set; }
        int AdjacentHeightIncrement { get; set; }
        private View View { get; set; }
        View AdjacentView { get; set; }
        Boolean Down { get; set; }

    //This constructor makes the animation start from height 0px
    public ResizeAnimation(View view, int offsetHeight, Boolean down) {
        this.View           = view;
        this.OriginalHeight = 0;
        this.TargetHeight   = 0;
        this.OffsetHeight   = offsetHeight;
        this.Down           = down;
    }
   
    //This constructor allow us to set a starting height
    public ResizeAnimation(View view, int originalHeight, int targetHeight, Boolean down) {
        this.View           = view;
        this.OriginalHeight = originalHeight;
        this.TargetHeight   = targetHeight;
        this.OffsetHeight   = targetHeight - originalHeight;
        this.Down           = down;
    }

    
    protected override void ApplyTransformation(float interpolatedTime, Transformation t) {
       // int newHeight;
        if (Down)
        {
            View.LayoutParameters.Height = (int) (OffsetHeight * interpolatedTime) + OriginalHeight;
        }
            
        else
        {
            View.LayoutParameters.Height = (int) (OffsetHeight * (1 - interpolatedTime)) + OriginalHeight;
            
        }
        View.RequestLayout();

    }

    
    public override void Initialize(int width, int height, int parentWidth,
            int parentHeight) {
        base.Initialize(width, height, parentWidth, parentHeight);
    }

    public override Boolean WillChangeBounds() {
        return true;
    }
   
    
    }
}