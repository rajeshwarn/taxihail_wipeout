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
using apcurium.MK.Booking.Mobile.Models;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Extensions;
using Android.Graphics;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Interfaces.Views;
using Android.Graphics.Drawables;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class PageInfo
    {
        public View RootView
        {
            get;
            set;
        }
        
        public View ContentView
        {
            get;
            set;
        }
        
        public TutorialItemModel ItemModel
        {
            get;
            set;
        }
        
        public bool IsLoaded
        {
            get;
            set;
        }
        
        
        
        
    }

    public class HorizontalPager : ViewGroup
    {
        private static int ANIMATION_SCREEN_SET_DURATION_MILLIS = 500;
        // What fraction (1/x) of the screen the user must swipe to indicate a page change
        private static int FRACTION_OF_SCREEN_WIDTH_FOR_SWIPE = 4;
        private static int INVALID_SCREEN = -1;
        // Argument to getVelocity for units to give pixels per second (1 = pixels per millisecond).
        private static int VELOCITY_UNIT_PIXELS_PER_SECOND = 1000;
        private static int TOUCH_STATE_REST = 0;
        private static int TOUCH_STATE_HORIZONTAL_SCROLLING = 1;
        private static int TOUCH_STATE_VERTICAL_SCROLLING = -1;
        private int mCurrentScreen;
        private int mDensityAdjustedSnapVelocity = 0;
        private bool mFirstLayout = true;
        private float mLastMotionX;
        private float mLastMotionY;

        public event EventHandler<ScreenSwitchArgs> mOnScreenSwitchListener;

        private int mMaximumVelocity;
        private int mNextScreen = INVALID_SCREEN;
        private Scroller mScroller;
        private int mTouchSlop;
        private int mTouchState = TOUCH_STATE_REST;
        private VelocityTracker mVelocityTracker;
        private int mLastSeenLayoutWidth = -1;
        private List<PageInfo> _pages;
        private TutorialItemModel[] _tutorialItemModel;

        public TutorialItemModel[] TutorialItemModel
        {
            get { return _tutorialItemModel; }
            set { _tutorialItemModel = value;  }
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



        private void UnloadItems()
        {
            if (_pages != null)
            {
                int i = 0;
                foreach (var page in _pages)
                {
                    if (page.IsLoaded)
                    {
                        UnloadItem(i);
                    }

                    i++;
                }
            }
            this.RemoveAllViews();
            _pages = null;
            GC.Collect();
        }

        void UnloadItem(int index)
        {
            if (index >= _pages.Count)
            {
                return;
            }

            var page = _pages[index];

            if (page.IsLoaded)
            {
                page.IsLoaded = false;

                if (page.ContentView != null)
                {
                    var d = page.ContentView.FindViewById<ImageView>(Resource.Id.TutorialImage).Drawable;
                    if (d is BitmapDrawable)
                    {
                        ((BitmapDrawable)d).Bitmap.Recycle();
                    }
                    page.ContentView.FindViewById<ImageView>(Resource.Id.TutorialImage).SetImageBitmap(null);

                    int i = this.IndexOfChild(page.ContentView);
                    this.RemoveView(page.ContentView);
                    page.RootView = new View(Context);
                    this.AddView(page.RootView, i);
                    page.RootView.Layout(page.ContentView.Left, 0, page.ContentView.Left + page.ContentView.MeasuredWidth, page.ContentView.MeasuredHeight);                                               
                    page.ContentView.Dispose();
                    page.ContentView = null;
                                     
                }

            }

            GC.Collect();


        }

        void LoadItem(int index)
        {

            if (index >= _pages.Count)
            {
                return;
            }

            GC.Collect();
            var page = _pages[index];
            if (!page.IsLoaded)
            {

                page.IsLoaded = true;
                var inflater = (LayoutInflater)Context.GetSystemService(Context.LayoutInflaterService);
                page.ContentView = inflater.Inflate(Resource.Layout.TutorialListItem, null);
                
                page.ContentView.FindViewById<TextView>(Resource.Id.TutorialTopText).Text = page.ItemModel.TopText;
                page.ContentView.FindViewById<TextView>(Resource.Id.TutorialBottomText).Text = page.ItemModel.BottomText;
                page.ContentView.FindViewById<TextView>(Resource.Id.TutorialTopTitleText).Text = page.ItemModel.TopTitle;
                page.ContentView.FindViewById<TextView>(Resource.Id.TutorialBottomTitleText).Text = page.ItemModel.BottomTitle;

                var resource = Resources.GetIdentifier(page.ItemModel.ImageUri, "drawable", Context.PackageName);

                //Decode image size
                var o = new BitmapFactory.Options();
                o.InPurgeable = true;
                o.InInputShareable = true;
                o.InPreferredConfig = Bitmap.Config.Rgb565;

                var bmp = BitmapFactory.DecodeResource(Resources, resource, o);

                page.ContentView.FindViewById<ImageView>(Resource.Id.TutorialImage).SetImageBitmap(bmp);
                page.ContentView.FindViewById<ImageView>(Resource.Id.TutorialImage).Invalidate();



                int i = this.IndexOfChild(page.RootView);
                this.RemoveView(page.RootView);
                this.AddView(page.ContentView, i);
                page.ContentView.Layout(page.RootView.Left, 0, page.RootView.Left + page.RootView.MeasuredWidth, page.RootView.MeasuredHeight);                           

                page.RootView.Dispose();
                page.RootView = null;

            }
        }
        
//        private void LayoutItem(int index)
//        {
//            var page = _pages[index];
//            if (page.ContentView != null)
//            {
//                page.ContentView.Layout(page.RootView.Left, 0, page.RootView.Left + page.RootView.MeasuredWidth, page.RootView.MeasuredHeight);
//            }
//            //page.ContentView.Layout( page.RootView.Left, 0,page.RootView.Left+page.RootView.MeasuredWidth, page.RootView.MeasuredWidth);
//            //            View child = _pages[i].RootView;
//            //            
//            //            if ((child != null) && (child.Visibilitpage.ContentView.Layout( page.RootView.Left, 0,page.RootView.Left+page.RootView.MeasuredWidth, page.RootView.MeasuredWidth);y != ViewStates.Gone))
//            //            {
//            //                int childWidth = child.MeasuredWidth;
//            //                child.Layout(childLeft, 0, childLeft + childWidth, child.MeasuredHeight);
//            //                childLeft += childWidth;
//            //            }
//            
//        }

        private void LoadItems()
        {
           
            if (_pages != null)
            {
                return;
            }

            _pages = new List<PageInfo>();                                           

            for (int i = 0; i < TutorialItemModel.Count(); i++)
            {
                var page = new PageInfo{ ItemModel =  TutorialItemModel[i], RootView = new View(Context) };
                _pages.Add(page);
                this.AddView(page.RootView);
            }
            LoadItem(0);
            PostDelayed(() => LoadItem(1), 200);
            mScroller = new Scroller(Context);

            ViewConfiguration configuration = ViewConfiguration.Get(Context);
            mTouchSlop = configuration.ScaledTouchSlop;
            mMaximumVelocity = configuration.ScaledMaximumFlingVelocity;
        }

        protected override void OnVisibilityChanged(View changedView, ViewStates visibility)
        {           
            base.OnVisibilityChanged(changedView, visibility);

            if (visibility == ViewStates.Visible)
            {
                LoadItems();
            }
            else
            {
                UnloadItems();
            }
        }

        protected override void OnWindowVisibilityChanged(ViewStates visibility)
        {
            base.OnWindowVisibilityChanged(visibility);
            if (visibility != ViewStates.Visible)
            {
                UnloadItems();
            }
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);


            int width = MeasureSpec.GetSize(widthMeasureSpec);
            MeasureSpecMode widthMode = MeasureSpec.GetMode(widthMeasureSpec);
            if (widthMode != MeasureSpecMode.Exactly)
            {
                //    throw new Exception("ViewSwitcher can only be used in EXACTLY mode.");
            }


            MeasureSpecMode heightMode = MeasureSpec.GetMode(heightMeasureSpec);
            if (heightMode != MeasureSpecMode.Exactly)
            {
                //  throw new Exception("ViewSwitcher can only be used in EXACTLY mode.");
            }


            // The children are given the same width and height as the workspace
            int count = ChildCount;
            for (int i = 0; i < count; i++)
            {
                GetChildAt(i).Measure(widthMeasureSpec, heightMeasureSpec);
            }


            if (mFirstLayout)
            {
                ScrollTo(mCurrentScreen * width, 0);
                mFirstLayout = false;
            }


            //else if (width != mLastSeenLayoutWidth)
            //{
            // Width has changed
            /*
                 * Recalculate the width and scroll to the right position to be sure we're in the right
                 * place in the event that we had a rotation that didn't result in an activity restart
                 * (code by aveyD). Without this you can end up between two pages after a rotation.
                 */
            /*var display = ((IWindowManager) Context.GetSystemService(Context.WindowService)).DefaultDisplay;
                int displayWidth = display.Width;


                mNextScreen = Math.Max(0, Math.Min(getCurrentScreen(), ChildCount - 1));
                int newX = mNextScreen*displayWidth;
                int delta = newX - ScrollX;


                mScroller.StartScroll(ScrollX, 0, delta, 0, 0);
            }*/


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
                        int xDiff = (int)Math.Abs(x - mLastMotionX);
                        bool xMoved = xDiff > mTouchSlop;


                        if (xMoved)
                        {
                            // Scroll if the user moved far enough along the X axis
                            mTouchState = TOUCH_STATE_HORIZONTAL_SCROLLING;
                            mLastMotionX = x;
                        }


                        float y = ev.GetY();
                        int yDiff = (int)Math.Abs(y - mLastMotionY);
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
                    int xDiff = (int)Math.Abs(x - mLastMotionX);
                    bool xMoved = xDiff > mTouchSlop;


                    if (xMoved)
                    {
                        // Scroll if the user moved far enough along the X axis
                        mTouchState = TOUCH_STATE_HORIZONTAL_SCROLLING;
                    }


                    if (mTouchState == TOUCH_STATE_HORIZONTAL_SCROLLING)
                    {
                        // Scroll to follow the motion event
                        int deltaX = (int)(mLastMotionX - x);
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
                        int velocityX = (int)velocityTracker.XVelocity;


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

                for (int i = 0; i < _pages.Count; i++)
                {
                    var idx = i;
                    if ((i == mCurrentScreen) || (i == mCurrentScreen - 1) || (i == mCurrentScreen + 1))
                    {
                        LoadItem(idx);
                    }
                    else
                    {
                        UnloadItem(idx);
                    }

                }


                // Notify observer about screen change
                if (mOnScreenSwitchListener != null)
                {
                    mOnScreenSwitchListener(this, new ScreenSwitchArgs(mCurrentScreen));
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
                ScrollTo(mCurrentScreen * Width, 0);
            }
            Invalidate();
        }


        /**
         * Sets the {@link OnScreenSwitchListener}.
         *
         * @param onScreenSwitchListener The listener for switch events.
         */

        


        /**
         * Snaps to the screen we think the user wants (the current screen for very small movements; the
         * next/prev screen for bigger movements).
         */

        private void snapToDestination()
        {
            int screenWidth = Width;
            int scrollX = ScrollX;
            int whichScreen = mCurrentScreen;
            int deltaX = scrollX - (screenWidth * mCurrentScreen);


            // Check if they want to go to the prev. screen
            if ((deltaX < 0) && mCurrentScreen != 0
                && ((screenWidth / FRACTION_OF_SCREEN_WIDTH_FOR_SWIPE) < -deltaX))
            {
                whichScreen--;
                // Check if they want to go to the next screen
            }
            else if ((deltaX > 0) && (mCurrentScreen + 1 != ChildCount)
                && ((screenWidth / FRACTION_OF_SCREEN_WIDTH_FOR_SWIPE) < deltaX))
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
            int newX = mNextScreen * Width;
            int delta = newX - ScrollX;


            if (duration < 0)
            {
                // E.g. if they've scrolled 80% of the way, only animation for 20% of the duration
                mScroller.StartScroll(ScrollX, 0, delta, 0, (int)(Math.Abs(delta)
                    / (float)Width * ANIMATION_SCREEN_SET_DURATION_MILLIS));
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

    public class ScreenSwitchArgs : EventArgs
    {
        public ScreenSwitchArgs(int mCurrentScreen)
        {
            Screen = mCurrentScreen;
        }

        public int Screen { get; set; }
    }
}
