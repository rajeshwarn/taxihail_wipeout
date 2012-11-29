using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class HorizontalPager : ViewGroup
    {
        /*
         * How long to animate between screens when programmatically setting with setCurrentScreen using
         * the animate parameter
         */
        private static int ANIMATION_SCREEN_SET_DURATION_MILLIS = 500;
        // What fraction (1/x) of the screen the user must swipe to indicate a page change
        private static int FRACTION_OF_SCREEN_WIDTH_FOR_SWIPE = 4;
        private static int INVALID_SCREEN = -1;
        /*
         * Velocity of a swipe (in density-independent pixels per second) to force a swipe to the
         * next/previous screen. Adjusted into mDensityAdjustedSnapVelocity on init.
         */
        private static int SNAP_VELOCITY_DIP_PER_SECOND = 600;
        // Argument to getVelocity for units to give pixels per second (1 = pixels per millisecond).
        private static int VELOCITY_UNIT_PIXELS_PER_SECOND = 1000;


        private static int TOUCH_STATE_REST = 0;
        private static int TOUCH_STATE_HORIZONTAL_SCROLLING = 1;
        private static int TOUCH_STATE_VERTICAL_SCROLLING = -1;
        private int mCurrentScreen;
        private int mDensityAdjustedSnapVelocity;
        private bool mFirstLayout = true;
        private float mLastMotionX;
        private float mLastMotionY;
        private OnScreenSwitchListener mOnScreenSwitchListener;
        private int mMaximumVelocity;
        private int mNextScreen = INVALID_SCREEN;
        private Scroller mScroller;
        private int mTouchSlop;
        private int mTouchState = TOUCH_STATE_REST;
        private VelocityTracker mVelocityTracker;
        private int mLastSeenLayoutWidth = -1;

        private List<TutorialItemModel> _tutorialItemModel;

        public List<TutorialItemModel> TutorialItemModel
        {
            get { return _tutorialItemModel; }
            set { _tutorialItemModel = value; this.init(); }
        }
        /**
         * Simple constructor to use when creating a view from code.
         *
         * @param context The Context the view is running in, through which it can
         *        access the current theme, resources, etc.
         */

        public HorizontalPager(Context context)
            : base(context)
        {
           // init();
        }


        /**
         * Constructor that is called when inflating a view from XML. This is called
         * when a view is being constructed from an XML file, supplying attributes
         * that were specified in the XML file. This version uses a default style of
         * 0, so the only attribute values applied are those in the Context's Theme
         * and the given AttributeSet.
         *
         * <p>
         * The method onFinishInflate() will be called after all children have been
         * added.
         *
         * @param context The Context the view is running in, through which it can
         *        access the current theme, resources, etc.
         * @param attrs The attributes of the XML tag that is inflating the view.
         * @see #View(Context, AttributeSet, int)
         */

        public HorizontalPager(Context context, IAttributeSet attrs)
            : base(context,attrs)
        {
            //init();
        }
        

        /**
         * Sets up the scroller and touch/fling sensitivity parameters for the pager.
         */

        private void init()
        {
            var viewOrder = new int[TutorialItemModel.Count];
            var inflater = (LayoutInflater)Context.GetSystemService(Context.LayoutInflaterService);
            for (int i = 0; i < TutorialItemModel.Count; i++)
            {
                View vw = inflater.Inflate(Resource.Layout.TutorialListItem, null);
                vw.Id = Resource.Layout.TutorialListItem + i;
                vw.FindViewById<TextView>(Resource.Id.TutorialTopText).Text = TutorialItemModel[i].TopText;
                vw.FindViewById<TextView>(Resource.Id.TutorialBottomText).Text = TutorialItemModel[i].BottomText;
                var resource = Resources.GetIdentifier(TutorialItemModel[i].ImageUri, "drawable", Context.PackageName);
                vw.FindViewById<ImageView>(Resource.Id.TutorialImage).SetImageResource(resource);
                this.AddView(vw);
                viewOrder[i] = Resource.Layout.TutorialListItem + i;

            }



            mScroller = new Scroller(Context);


            // Calculate the density-dependent snap velocity in pixels
            DisplayMetrics displayMetrics = new DisplayMetrics();
            ((IWindowManager) Context.GetSystemService(Context.WindowService)).DefaultDisplay.GetMetrics(displayMetrics);
            mDensityAdjustedSnapVelocity = (int) (displayMetrics.Density*SNAP_VELOCITY_DIP_PER_SECOND);


            ViewConfiguration configuration = ViewConfiguration.Get(Context);
            mTouchSlop = configuration.ScaledTouchSlop;
            mMaximumVelocity = configuration.ScaledMaximumFlingVelocity;
        }



        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);


            int width = MeasureSpec.GetSize(widthMeasureSpec);
            MeasureSpecMode widthMode = MeasureSpec.GetMode(widthMeasureSpec);
            if (widthMode != MeasureSpecMode.Exactly)
            {
                throw new Exception("ViewSwitcher can only be used in EXACTLY mode.");
            }


            MeasureSpecMode heightMode = MeasureSpec.GetMode(heightMeasureSpec);
            if (heightMode != MeasureSpecMode.Exactly)
            {
                throw new Exception("ViewSwitcher can only be used in EXACTLY mode.");
            }


            // The children are given the same width and height as the workspace
            int count = ChildCount;
            for (int i = 0; i < count; i++)
            {
                GetChildAt(i).Measure(widthMeasureSpec, heightMeasureSpec);
            }


            if (mFirstLayout)
            {
                ScrollTo(mCurrentScreen*width, 0);
                mFirstLayout = false;
            }


            else if (width != mLastSeenLayoutWidth)
            {
                // Width has changed
                /*
                 * Recalculate the width and scroll to the right position to be sure we're in the right
                 * place in the event that we had a rotation that didn't result in an activity restart
                 * (code by aveyD). Without this you can end up between two pages after a rotation.
                 */
                Display display =
                    ((IWindowManager) Context.GetSystemService(Context.WindowService)).DefaultDisplay;
                int displayWidth = display.Width;


                mNextScreen = Math.Max(0, Math.Min(getCurrentScreen(), ChildCount - 1));
                int newX = mNextScreen*displayWidth;
                int delta = newX - ScrollX;


                mScroller.StartScroll(ScrollX, 0, delta, 0, 0);
            }


            mLastSeenLayoutWidth = width;
        }



        protected override void OnLayout(bool changed, int l, int t, int r,
                                         int b)
        {
            int childLeft = 0;
            int count = ChildCount;


            for (int i = 0; i < count; i++)
            {
                View child = GetChildAt(i);
                if (child.Visibility != ViewStates.Gone)
                {
                    int childWidth = child.MeasuredWidth;
                    child.Layout(childLeft, 0, childLeft + childWidth, child.MeasuredHeight);
                    childLeft += childWidth;
                }
            }
        }



        public override bool OnInterceptTouchEvent(MotionEvent ev)
        {
            /*
             * By Yoni Samlan: Modified onInterceptTouchEvent based on standard ScrollView's
             * onIntercept. The logic is designed to support a nested vertically scrolling view inside
             * this one; once a scroll registers for X-wise scrolling, handle it in this view and don't
             * let the children, but once a scroll registers for y-wise scrolling, let the children
             * handle it exclusively.
             */
            MotionEventActions action = ev.Action;
            bool intercept = false;


            switch (action)
            {
                case MotionEventActions.Move:
                    /*
                     * If we're in a horizontal scroll event, take it (intercept further events). But if
                     * we're mid-vertical-scroll, don't even try; let the children deal with it. If we
                     * haven't found a scroll event yet, check for one.
                     */
                    if (mTouchState == TOUCH_STATE_HORIZONTAL_SCROLLING)
                    {
                        /*
                         * We've already started a horizontal scroll; set intercept to true so we can
                         * take the remainder of all touch events in onTouchEvent.
                         */
                        intercept = true;
                    }
                    else if (mTouchState == TOUCH_STATE_VERTICAL_SCROLLING)
                    {
                        // Let children handle the events for the duration of the scroll event.
                        intercept = false;
                    }
                    else
                    {
                        // We haven't picked up a scroll event yet; check for one.


                        /*
                         * If we detected a horizontal scroll event, start stealing touch events (mark
                         * as scrolling). Otherwise, see if we had a vertical scroll event -- if so, let
                         * the children handle it and don't look to intercept again until the motion is
                         * done.
                         */


                        float x = ev.GetX();
                        int xDiff = (int) Math.Abs(x - mLastMotionX);
                        bool xMoved = xDiff > mTouchSlop;


                        if (xMoved)
                        {
                            // Scroll if the user moved far enough along the X axis
                            mTouchState = TOUCH_STATE_HORIZONTAL_SCROLLING;
                            mLastMotionX = x;
                        }


                        float y = ev.GetY();
                        int yDiff = (int) Math.Abs(y - mLastMotionY);
                        bool yMoved = yDiff > mTouchSlop;


                        if (yMoved)
                        {
                            mTouchState = TOUCH_STATE_VERTICAL_SCROLLING;
                        }
                    }


                    break;
                case MotionEventActions.Cancel:
                case MotionEventActions.Up:
                    // Release the drag.
                    mTouchState = TOUCH_STATE_REST;
                    break;
                case MotionEventActions.Down:
                    /*
                     * No motion yet, but register the coordinates so we can check for intercept at the
                     * next MOVE event.
                     */
                    mLastMotionY = ev.GetY();
                    mLastMotionX = ev.GetX();
                    break;
                default:
                    break;
            }


            return intercept;
        }


        public override bool OnTouchEvent(MotionEvent ev)
        {


            if (mVelocityTracker == null)
            {
                mVelocityTracker = VelocityTracker.Obtain();
            }
            mVelocityTracker.AddMovement(ev);


            MotionEventActions action = ev.Action;
            float x = ev.GetX();


            switch (action)
            {
                case MotionEventActions.Down:
                    /*
                     * If being flinged and user touches, stop the fling. isFinished will be false if
                     * being flinged.
                     */
                    if (!mScroller.IsFinished)
                    {
                        mScroller.AbortAnimation();
                    }


                    // Remember where the motion event started
                    mLastMotionX = x;


                    if (mScroller.IsFinished)
                    {
                        mTouchState = TOUCH_STATE_REST;
                    }
                    else
                    {
                        mTouchState = TOUCH_STATE_HORIZONTAL_SCROLLING;
                    }


                    break;
                case MotionEventActions.Move:
                    int xDiff = (int) Math.Abs(x - mLastMotionX);
                    bool xMoved = xDiff > mTouchSlop;


                    if (xMoved)
                    {
                        // Scroll if the user moved far enough along the X axis
                        mTouchState = TOUCH_STATE_HORIZONTAL_SCROLLING;
                    }


                    if (mTouchState == TOUCH_STATE_HORIZONTAL_SCROLLING)
                    {
                        // Scroll to follow the motion event
                        int deltaX = (int) (mLastMotionX - x);
                        mLastMotionX = x;
                        int scrollX = ScrollX;


                        if (deltaX < 0)
                        {
                            if (scrollX > 0)
                            {
                                ScrollBy(Math.Max(-scrollX, deltaX), 0);
                            }
                        }
                        else if (deltaX > 0)
                        {
                            int availableToScroll =
                                GetChildAt(ChildCount - 1).Right - scrollX - Width;


                            if (availableToScroll > 0)
                            {
                                ScrollBy(Math.Min(availableToScroll, deltaX), 0);
                            }
                        }
                    }


                    break;


                case MotionEventActions.Up:
                    if (mTouchState == TOUCH_STATE_HORIZONTAL_SCROLLING)
                    {
                        VelocityTracker velocityTracker = mVelocityTracker;
                        velocityTracker.ComputeCurrentVelocity(VELOCITY_UNIT_PIXELS_PER_SECOND,
                                                               mMaximumVelocity);
                        int velocityX = (int) velocityTracker.XVelocity;


                        if (velocityX > mDensityAdjustedSnapVelocity && mCurrentScreen > 0)
                        {
                            // Fling hard enough to move left
                            snapToScreen(mCurrentScreen - 1);
                        }
                        else if (velocityX < -mDensityAdjustedSnapVelocity
                                 && mCurrentScreen < ChildCount - 1)
                        {
                            // Fling hard enough to move right
                            snapToScreen(mCurrentScreen + 1);
                        }
                        else
                        {
                            snapToDestination();
                        }


                        if (mVelocityTracker != null)
                        {
                            mVelocityTracker.Recycle();
                            mVelocityTracker = null;
                        }
                    }


                    mTouchState = TOUCH_STATE_REST;


                    break;
                case MotionEventActions.Cancel:
                    mTouchState = TOUCH_STATE_REST;
                    break;
                default:
                    break;
            }


            return true;
        }



        public override void ComputeScroll()
        {
            if (mScroller.ComputeScrollOffset())
            {
                ScrollTo(mScroller.CurrX, mScroller.CurrY);
                PostInvalidate();
            }
            else if (mNextScreen != INVALID_SCREEN)
            {
                mCurrentScreen = Math.Max(0, Math.Min(mNextScreen, ChildCount - 1));


                // Notify observer about screen change
                if (mOnScreenSwitchListener != null)
                {
                    mOnScreenSwitchListener.onScreenSwitched(mCurrentScreen);
                }


                mNextScreen = INVALID_SCREEN;
            }
        }


        /**
         * Returns the index of the currently displayed screen.
         *
         * @return The index of the currently displayed screen.
         */

        public int getCurrentScreen()
        {
            return mCurrentScreen;
        }


        /**
         * Sets the current screen.
         *
         * @param currentScreen The new screen.
         * @param animate True to smoothly scroll to the screen, false to snap instantly
         */

        public void setCurrentScreen(int currentScreen, bool animate)
        {
            mCurrentScreen = Math.Max(0, Math.Min(currentScreen, ChildCount - 1));
            if (animate)
            {
                snapToScreen(currentScreen, ANIMATION_SCREEN_SET_DURATION_MILLIS);
            }
            else
            {
                ScrollTo(mCurrentScreen*Width, 0);
            }
            Invalidate();
        }


        /**
         * Sets the {@link OnScreenSwitchListener}.
         *
         * @param onScreenSwitchListener The listener for switch events.
         */

        public void setOnScreenSwitchListener(OnScreenSwitchListener onScreenSwitchListener)
        {
            mOnScreenSwitchListener = onScreenSwitchListener;
        }


        /**
         * Snaps to the screen we think the user wants (the current screen for very small movements; the
         * next/prev screen for bigger movements).
         */

        private void snapToDestination()
        {
            int screenWidth = Width;
            int scrollX = ScrollX;
            int whichScreen = mCurrentScreen;
            int deltaX = scrollX - (screenWidth*mCurrentScreen);


            // Check if they want to go to the prev. screen
            if ((deltaX < 0) && mCurrentScreen != 0
                && ((screenWidth/FRACTION_OF_SCREEN_WIDTH_FOR_SWIPE) < -deltaX))
            {
                whichScreen--;
                // Check if they want to go to the next screen
            }
            else if ((deltaX > 0) && (mCurrentScreen + 1 != ChildCount)
                     && ((screenWidth/FRACTION_OF_SCREEN_WIDTH_FOR_SWIPE) < deltaX))
            {
                whichScreen++;
            }


            snapToScreen(whichScreen);
        }


        /**
         * Snap to a specific screen, animating automatically for a duration proportional to the
         * distance left to scroll.
         *
         * @param whichScreen Screen to snap to
         */

        private void snapToScreen(int whichScreen)
        {
            snapToScreen(whichScreen, -1);
        }


        /**
         * Snaps to a specific screen, animating for a specific amount of time to get there.
         *
         * @param whichScreen Screen to snap to
         * @param duration -1 to automatically time it based on scroll distance; a positive number to
         *            make the scroll take an exact duration.
         */

        private void snapToScreen(int whichScreen, int duration)
        {
            /*
             * Modified by Yoni Samlan: Allow new snapping even during an ongoing scroll animation. This
             * is intended to make HorizontalPager work as expected when used in conjunction with a
             * RadioGroup used as "tabbed" controls. Also, make the animation take a percentage of our
             * normal animation time, depending how far they've already scrolled.
             */
            mNextScreen = Math.Max(0, Math.Min(whichScreen, ChildCount - 1));
            int newX = mNextScreen*Width;
            int delta = newX - ScrollX;


            if (duration < 0)
            {
                // E.g. if they've scrolled 80% of the way, only animation for 20% of the duration
                mScroller.StartScroll(ScrollX, 0, delta, 0, (int) (Math.Abs(delta)
                                                                   /(float) Width*ANIMATION_SCREEN_SET_DURATION_MILLIS));
            }
            else
            {
                mScroller.StartScroll(ScrollX, 0, delta, 0, duration);
            }


            Invalidate();
        }


        /**
         * Listener for the event that the HorizontalPager switches to a new view.
         */

        public interface OnScreenSwitchListener
        {
            /**
             * Notifies listeners about the new screen. Runs after the animation completed.
             *
             * @param screen The new screen index.
             */
            void onScreenSwitched(int screen);
        }
    }
}
