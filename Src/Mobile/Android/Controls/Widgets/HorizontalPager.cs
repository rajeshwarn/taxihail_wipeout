using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Models;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class PageInfo
    {
        public View RootView { get; set; }

        public View ContentView { get; set; }

        public TutorialItemModel ItemModel { get; set; }

        public bool IsLoaded { get; set; }
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
        private List<PageInfo> _pages;
        private TutorialItemModel[] _tutorialItemModel;
        private int _mCurrentScreen;
        private int mDensityAdjustedSnapVelocity = 0;
        private bool _mFirstLayout = true;
        private float _mLastMotionX;
        private float _mLastMotionY;

        private int _mMaximumVelocity;
        private int _mNextScreen = INVALID_SCREEN;
        private Scroller _mScroller;
        private int _mTouchSlop;
        private int _mTouchState = TOUCH_STATE_REST;
        private VelocityTracker _mVelocityTracker;

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
            : base(context, attrs)
        {
            //init();
        }

        public TutorialItemModel[] TutorialItemModel
        {
            get { return _tutorialItemModel; }
// ReSharper disable once UnusedMember.Global
            set { _tutorialItemModel = value; }
        }

        public event EventHandler<ScreenSwitchArgs> MOnScreenSwitchListener;


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
            RemoveAllViews();
            _pages = null;
            GC.Collect();
        }

        private void UnloadItem(int index)
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
                    var drawable = d as BitmapDrawable;
                    if (drawable != null && drawable.Bitmap != null)
                    {
                        drawable.Bitmap.Recycle();
                    }
                    page.ContentView.FindViewById<ImageView>(Resource.Id.TutorialImage).SetImageBitmap(null);

                    int i = IndexOfChild(page.ContentView);
                    RemoveView(page.ContentView);
                    page.RootView = new View(Context);

                    AddView(page.RootView, i);
                    page.RootView.Layout(page.ContentView.Left, 0,
                        page.ContentView.Left + page.ContentView.MeasuredWidth, page.ContentView.MeasuredHeight);
                    page.ContentView.Dispose();
                    page.ContentView = null;
                }
            }

            
        }

        private void LoadItem(int index)
        {
            if (index >= _pages.Count)
            {
                return;
            }

            
            var page = _pages[index];
            if (!page.IsLoaded)
            {
                page.IsLoaded = true;
                var inflater = (LayoutInflater) Context.GetSystemService(Context.LayoutInflaterService);
                page.ContentView = inflater.Inflate(Resource.Layout.TutorialListItem, null);

                page.ContentView.FindViewById<TextView>(Resource.Id.TutorialText).Text = page.ItemModel.Text;
                page.ContentView.FindViewById<TextView>(Resource.Id.TutorialTitle).Text = page.ItemModel.Title;
                var resource = Resources.GetIdentifier(page.ItemModel.ImageUri, "drawable", Context.PackageName);

                //Decode image size
                var o = new BitmapFactory.Options();
                o.InPurgeable = true;
                o.InInputShareable = true;
                o.InPreferredConfig = Bitmap.Config.Rgb565;

                var bmp = BitmapFactory.DecodeResource(Resources, resource, o);

                page.ContentView.FindViewById<ImageView>(Resource.Id.TutorialImage).SetImageBitmap(bmp);
                page.ContentView.FindViewById<ImageView>(Resource.Id.TutorialImage).Invalidate();

                int i = IndexOfChild(page.RootView);
                RemoveView(page.RootView);
                AddView(page.ContentView, i);
                page.ContentView.Layout(page.RootView.Left, 0, page.RootView.Left + page.RootView.MeasuredWidth,
                    page.RootView.MeasuredHeight);

                page.RootView.Dispose();
                page.RootView = null;
            }
        }

        private void LoadItems()
        {
            if (_pages != null)
            {
                return;
            }

            _pages = new List<PageInfo>();

            for (int i = 0; i < TutorialItemModel.Count(); i++)
            {
                var page = new PageInfo {ItemModel = TutorialItemModel[i], RootView = new View(Context)};
                _pages.Add(page);
                AddView(page.RootView);
            }

            LoadItem(0);
            PostDelayed(() => LoadItem(1), 200);
            _mScroller = new Scroller(Context);

            var configuration = ViewConfiguration.Get(Context);
            _mTouchSlop = configuration.ScaledTouchSlop;
            _mMaximumVelocity = configuration.ScaledMaximumFlingVelocity;
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

            var widthMode = MeasureSpec.GetMode(widthMeasureSpec);
            if (widthMode != MeasureSpecMode.Exactly)
            {
                //    throw new Exception("ViewSwitcher can only be used in EXACTLY mode.");
            }

            var heightMode = MeasureSpec.GetMode(heightMeasureSpec);
            if (heightMode != MeasureSpecMode.Exactly)
            {
                //  throw new Exception("ViewSwitcher can only be used in EXACTLY mode.");
            }

            // The children are given the same width and height as the workspace
            for (int i = 0; i < ChildCount; i++)
            {
                GetChildAt(i).Measure(widthMeasureSpec, heightMeasureSpec);
            }
                
            if (_mFirstLayout)
            {
                int width = MeasureSpec.GetSize(widthMeasureSpec);
                ScrollTo(_mCurrentScreen * width, 0);
                _mFirstLayout = false;
            }
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            var childLeft = 0;

            for (int i = 0; i < ChildCount; i++)
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
            var action = ev.Action;
            var intercept = false;

            switch (action)
            {
                case MotionEventActions.Move:
                    /*
                     * If we're in a horizontal scroll event, take it (intercept further events). But if
                     * we're mid-vertical-scroll, don't even try; let the children deal with it. If we
                     * haven't found a scroll event yet, check for one.
                     */
                    if (_mTouchState == TOUCH_STATE_HORIZONTAL_SCROLLING)
                    {
                        /*
                         * We've already started a horizontal scroll; set intercept to true so we can
                         * take the remainder of all touch events in onTouchEvent.
                         */
                        intercept = true;
                    }
                    else if (_mTouchState == TOUCH_STATE_VERTICAL_SCROLLING)
                    {
                        // Let children handle the events for the duration of the scroll event.
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
                        var xDiff = (int) Math.Abs(x - _mLastMotionX);
                        bool xMoved = xDiff > _mTouchSlop;


                        if (xMoved)
                        {
                            // Scroll if the user moved far enough along the X axis
                            _mTouchState = TOUCH_STATE_HORIZONTAL_SCROLLING;
                            _mLastMotionX = x;
                        }


                        float y = ev.GetY();
                        var yDiff = (int) Math.Abs(y - _mLastMotionY);
                        bool yMoved = yDiff > _mTouchSlop;


                        if (yMoved)
                        {
                            _mTouchState = TOUCH_STATE_VERTICAL_SCROLLING;
                        }
                    }


                    break;
                case MotionEventActions.Cancel:
                case MotionEventActions.Up:
                    // Release the drag.
                    _mTouchState = TOUCH_STATE_REST;
                    break;
                case MotionEventActions.Down:
                    /*
                     * No motion yet, but register the coordinates so we can check for intercept at the
                     * next MOVE event.
                     */
                    _mLastMotionY = ev.GetY();
                    _mLastMotionX = ev.GetX();
                    break;
            }

            return intercept;
        }

        public override bool OnTouchEvent(MotionEvent ev)
        {
            if (_mVelocityTracker == null)
            {
                _mVelocityTracker = VelocityTracker.Obtain();
            }
            _mVelocityTracker.AddMovement(ev);


            MotionEventActions action = ev.Action;
            float x = ev.GetX();


            switch (action)
            {
                case MotionEventActions.Down:
                    /*
                     * If being flinged and user touches, stop the fling. isFinished will be false if
                     * being flinged.
                     */
                    if (!_mScroller.IsFinished)
                    {
                        _mScroller.AbortAnimation();
                    }


                    // Remember where the motion event started
                    _mLastMotionX = x;


                    _mTouchState = _mScroller.IsFinished ? TOUCH_STATE_REST : TOUCH_STATE_HORIZONTAL_SCROLLING;


                    break;
                case MotionEventActions.Move:
                    var xDiff = (int) Math.Abs(x - _mLastMotionX);
                    bool xMoved = xDiff > _mTouchSlop;


                    if (xMoved)
                    {
                        // Scroll if the user moved far enough along the X axis
                        _mTouchState = TOUCH_STATE_HORIZONTAL_SCROLLING;
                    }


                    if (_mTouchState == TOUCH_STATE_HORIZONTAL_SCROLLING)
                    {
                        // Scroll to follow the motion event
                        var deltaX = (int) (_mLastMotionX - x);
                        _mLastMotionX = x;
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
                    if (_mTouchState == TOUCH_STATE_HORIZONTAL_SCROLLING)
                    {
                        VelocityTracker velocityTracker = _mVelocityTracker;
                        velocityTracker.ComputeCurrentVelocity(VELOCITY_UNIT_PIXELS_PER_SECOND,
                            _mMaximumVelocity);
                        var velocityX = (int) velocityTracker.XVelocity;


                        if (velocityX > mDensityAdjustedSnapVelocity && _mCurrentScreen > 0)
                        {
                            // Fling hard enough to move left
                            SnapToScreen(_mCurrentScreen - 1);
                        }
                        else if (velocityX < -mDensityAdjustedSnapVelocity
                                 && _mCurrentScreen < ChildCount - 1)
                        {
                            // Fling hard enough to move right
                            SnapToScreen(_mCurrentScreen + 1);
                        }
                        else
                        {
                            SnapToDestination();
                        }


                        if (_mVelocityTracker != null)
                        {
                            _mVelocityTracker.Recycle();
                            _mVelocityTracker = null;
                        }
                    }


                    _mTouchState = TOUCH_STATE_REST;


                    break;
                case MotionEventActions.Cancel:
                    _mTouchState = TOUCH_STATE_REST;
                    break;
            }


            return true;
        }

        public override void ComputeScroll()
        {
            if (_mScroller.ComputeScrollOffset())
            {
                ScrollTo(_mScroller.CurrX, _mScroller.CurrY);
                PostInvalidate();
            }
            else if (_mNextScreen != INVALID_SCREEN)
            {
                _mCurrentScreen = Math.Max(0, Math.Min(_mNextScreen, ChildCount - 1));

                for (int i = 0; i < _pages.Count; i++)
                {
                    var idx = i;
                    if ((i == _mCurrentScreen) || (i == _mCurrentScreen - 1) || (i == _mCurrentScreen + 1))
                    {
                        LoadItem(idx);
                    }
                    else
                    {
                        UnloadItem(idx);
                    }
                }


                // Notify observer about screen change
                if (MOnScreenSwitchListener != null)
                {
                    MOnScreenSwitchListener(this, new ScreenSwitchArgs(_mCurrentScreen));
                }


                _mNextScreen = INVALID_SCREEN;
            }
        }


        /**
         * Returns the index of the currently displayed screen.
         *
         * @return The index of the currently displayed screen.
         */

        public int GetCurrentScreen()
        {
            return _mCurrentScreen;
        }


        /**
         * Sets the current screen.
         *
         * @param currentScreen The new screen.
         * @param animate True to smoothly scroll to the screen, false to snap instantly
         */

        public void SetCurrentScreen(int currentScreen, bool animate)
        {
            _mCurrentScreen = Math.Max(0, Math.Min(currentScreen, ChildCount - 1));
            if (animate)
            {
                SnapToScreen(currentScreen, ANIMATION_SCREEN_SET_DURATION_MILLIS);
            }
            else
            {
                ScrollTo(_mCurrentScreen*Width, 0);
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

        private void SnapToDestination()
        {
            int screenWidth = Width;
            int scrollX = ScrollX;
            int whichScreen = _mCurrentScreen;
            int deltaX = scrollX - (screenWidth*_mCurrentScreen);


            // Check if they want to go to the prev. screen
            if ((deltaX < 0) && _mCurrentScreen != 0
                && ((screenWidth/FRACTION_OF_SCREEN_WIDTH_FOR_SWIPE) < -deltaX))
            {
                whichScreen--;
                // Check if they want to go to the next screen
            }
            else if ((deltaX > 0) && (_mCurrentScreen + 1 != ChildCount)
                     && ((screenWidth/FRACTION_OF_SCREEN_WIDTH_FOR_SWIPE) < deltaX))
            {
                whichScreen++;
            }

            SnapToScreen(whichScreen);
        }


        /**
         * Snap to a specific screen, animating automatically for a duration proportional to the
         * distance left to scroll.
         *
         * @param whichScreen Screen to snap to
         */

        private void SnapToScreen(int whichScreen)
        {
            SnapToScreen(whichScreen, -1);
        }


        /**
         * Snaps to a specific screen, animating for a specific amount of time to get there.
         *
         * @param whichScreen Screen to snap to
         * @param duration -1 to automatically time it based on scroll distance; a positive number to
         *            make the scroll take an exact duration.
         */

        private void SnapToScreen(int whichScreen, int duration)
        {
            /*
             * Modified by Yoni Samlan: Allow new snapping even during an ongoing scroll animation. This
             * is intended to make HorizontalPager work as expected when used in conjunction with a
             * RadioGroup used as "tabbed" controls. Also, make the animation take a percentage of our
             * normal animation time, depending how far they've already scrolled.
             */
            _mNextScreen = Math.Max(0, Math.Min(whichScreen, ChildCount - 1));
            int newX = _mNextScreen*Width;
            int delta = newX - ScrollX;


            if (duration < 0)
            {
                // E.g. if they've scrolled 80% of the way, only animation for 20% of the duration
                _mScroller.StartScroll(ScrollX, 0, delta, 0, (int) (Math.Abs(delta)
                                                                   /(float) Width*ANIMATION_SCREEN_SET_DURATION_MILLIS));
            }
            else
            {
                _mScroller.StartScroll(ScrollX, 0, delta, 0, duration);
            }


            Invalidate();
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