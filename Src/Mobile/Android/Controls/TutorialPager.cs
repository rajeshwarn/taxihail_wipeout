using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.IO;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class TutorialPager : ViewFlipper
    {
        private static String LOG_TAG = "MyApp";

        private static int MENU_VIEWFLIP_ANIM = Menu.First;
        private static int MENU_MOVE_NEXT = Menu.First + 1;
        private static int MENU_MOVE_CURRENT = Menu.First + 2;

        private static int procMode = MENU_VIEWFLIP_ANIM;

        private static int SWITCH_THRESHOLD = 10;

        private static int FLIPMODE_NOMOVE = 0;
        private static int FLIPMODE_NEXT = 1;
        private static int FLIPMODE_PREV = -1;
        private static int flipMode = FLIPMODE_NOMOVE;

        private View currentView = null;
        private View nextView = null;
        private View prevView = null;

        private int[] viewOrder = null;
        private int curIdx = -1;
        private int preIdx = -1;
        private int nxtIdx = -1;

        private int movePageThreshold = 0;
        private float startX;
        private List<TutorialItemModel> _tutorialItemModel;

        public List<TutorialItemModel> TutorialItemModel
        {
            get { return _tutorialItemModel; }
            set { _tutorialItemModel = value; this.Execute();}
        }

        protected TutorialPager(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
            Initialize();
        }

        public TutorialPager(Context context)
            : base(context)
        {
            Initialize();
        }

        public TutorialPager(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            Initialize();
        }

        private void Initialize()
        {
         /*   int[] layouts = new int[]
                                {
                                    Resource.Layout.layout_page1,
                                    Resource.Layout.layout_page2,
                                    Resource.Layout.layout_page3
                                };
            viewOrder = new int[layouts.Length];
            var inflater = (LayoutInflater) Context.GetSystemService(Context.LayoutInflaterService);

            for (int i = 0; i < layouts.Length; i++)
            {
                View vw = inflater.Inflate(layouts[i], null);
                vw.
                vw.Id = layouts[i];
                this.AddView(vw);
                viewOrder[i] = layouts[i];
            }*/
            /*var inflater = (LayoutInflater) Context.GetSystemService(Context.LayoutInflaterService);
            for (int i = 0; i < TutorialItemModel.Count; i++)
            {
                View vw = inflater.Inflate(Resource.Layout.TutorialListItem, null);
                vw.Id = Resource.Layout.TutorialListItem;
                this.AddView(vw);
                viewOrder[i] = Resource.Layout.TutorialListItem;
            }*/
        }

        private void Execute()
        {
            this.DrawingCacheBackgroundColor = Color.Transparent;
            viewOrder = new int[TutorialItemModel.Count];
            var inflater = (LayoutInflater)Context.GetSystemService(Context.LayoutInflaterService);
            for (int i = 0; i < TutorialItemModel.Count; i++)
            {
                View vw = inflater.Inflate(Resource.Layout.TutorialListItem, null);
                vw.Id = Resource.Layout.TutorialListItem+i;
                vw.FindViewById<TextView>(Resource.Id.TutorialText).Text = TutorialItemModel[i].Text;
                var resource = Resources.GetIdentifier(TutorialItemModel[i].ImageUri, "drawable", Context.PackageName);
                vw.FindViewById<ImageView>(Resource.Id.TutorialImage).SetImageResource(resource);
                this.AddView(vw);
                viewOrder[i] = Resource.Layout.TutorialListItem+i;

            }
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            bool ret = false;
            /*        switch(procMode) {
                case MENU_VIEWFLIP_ANIM:
                    ret =  pageFlipWithSimpleAnimation(e);
                    break;
                case MENU_MOVE_CURRENT:
                    ret = pageFlipWithFingerMoveCurrent(e);
                    break;
                case MENU_MOVE_NEXT:*/
            ret = pageFlipWithFingerMoveNext(e);
          //  ret = pageFlipWithFingerMoveCurrent(e);
            //break;
            // }
            return ret;
        }



        public bool pageFlipWithSimpleAnimation(MotionEvent @event)
        {
            switch (@event.Action)
            {
                case MotionEventActions.Down:
                    startX = @event.GetX();
                    break;
                case MotionEventActions.Up:
                    float currentX = @event.GetX();
                    if (this.startX > currentX)
                    {
                        //  vf.SetInAnimation(AnimationUtils.LoadAnimation(this, Resource.Layout.push_left_in));
                        //vf.SetOutAnimation(AnimationUtils.LoadAnimation(this, Resource.Layout.push_left_out));
                        this.ShowNext();
                    }
                    if (this.startX < currentX)
                    {
                        //vf.SetInAnimation(AnimationUtils.LoadAnimation(this, Resource.Layout.push_right_out));
                        //vf.SetOutAnimation(AnimationUtils.LoadAnimation(this, Resource.Layout.push_right_in));
                        this.ShowPrevious();
                    }
                    break;
                default:
                    break;
            }
            return true;
        }

        public bool pageFlipWithFingerMoveNext(MotionEvent e)
        {
            float currentX = e.GetX();
            switch (e.Action)
            {
                case MotionEventActions.Down:
                    startX = e.GetX();

                    currentView = this.CurrentView;
                    movePageThreshold = (currentView.Width/5);

                    int viewCount = viewOrder.Length;
                    for (int i = 0; i < viewCount; i++)
                    {
                        //Log.i("MyApp", "ord=id:" + viewOrder[i] + "==" + cv.getId());
                        if (viewOrder[i] == currentView.Id)
                        {
                            curIdx = i;
                            break;
                        }
                    }

                    if (curIdx >= 0)
                    {
                        preIdx = curIdx - 1;
                        nxtIdx = curIdx + 1;
                        preIdx = (preIdx < 0) ? viewCount - 1 : preIdx;
                        nxtIdx = (nxtIdx >= viewCount) ? 0 : nxtIdx;

                        prevView = this.FindViewById(viewOrder[preIdx]);
                        nextView = this.FindViewById(viewOrder[nxtIdx]);
                        
                    }

                    Log.Info(LOG_TAG,
                             String.Format("Pre=%d(%d),Cur=%d(%d),Nxt=%d(%d)"
                                           , preIdx, prevView.Id
                                           , curIdx, currentView.Id
                                           , nxtIdx, nextView.Id));



                    break;
                case MotionEventActions.Move:
                    int travelDistanceX = (int) (currentX - this.startX);
                    int fingerPosX = (int) currentX;

                    if (flipMode == FLIPMODE_NOMOVE)
                    {
                        if (travelDistanceX > SWITCH_THRESHOLD)
                        {
                            flipMode = FLIPMODE_PREV;
                        }
                        else if
                            (travelDistanceX < (SWITCH_THRESHOLD*-1))
                        {
                            flipMode = FLIPMODE_NEXT;
                        }
                        else
                        {
                            flipMode = FLIPMODE_NOMOVE;
                        }
                    }

                    if (flipMode == FLIPMODE_PREV)
                    {
                        prevView.Visibility = ViewStates.Invisible;
                        prevView.Layout(fingerPosX - prevView.Width,
                                        prevView.Top,
                                        fingerPosX,
                                        prevView.Bottom);
                        
                        prevView.Visibility = ViewStates.Visible;
                        //this.BringChildToFront(prevView);


                    }
                    if (flipMode == FLIPMODE_NEXT)
                    {
                        nextView.Visibility = ViewStates.Invisible;
                        nextView.Layout(fingerPosX,
                                        nextView.Top,
                                        fingerPosX + currentView.Width + nextView.Width,
                                        nextView.Bottom);
                       
                        nextView.Visibility = ViewStates.Visible;
                        //this.BringChildToFront(nextView);
                        
                    }
                    break;

                case MotionEventActions.Up:

                    int activeIdx = -1;
                    if ((this.startX - currentX) > movePageThreshold)
                    {
                        activeIdx = nxtIdx;
                    }
                    else if
                        ((this.startX - currentX) < (movePageThreshold*-1))
                    {
                        activeIdx = preIdx;
                    }
                    else
                    {
                        activeIdx = curIdx;
                    }
                    int activeId = viewOrder[activeIdx];
                    for (int i = 0; i < this.ChildCount; i++)
                    {
                        // Log.i("MyApp",String.format("vf_id:%d,sel_id:%d",vf.getChildAt(i).getId(),activeId));
                        if (this.GetChildAt(i).Id == activeId)
                        {
                            this.DisplayedChild = i;
                            break;
                        }
                    }
                    flipMode = 0;
                    break;
                default:
                    break;
            }
            return true;
        }


        public bool pageFlipWithFingerMoveCurrent(MotionEvent e)
        {
            float currentX = e.GetX();

            switch (e.Action)
            {
                case MotionEventActions.Down:
                    startX = e.GetX();

                    currentView = this.CurrentView;
                    movePageThreshold = (currentView.Width/5);

                    int viewCount = viewOrder.Length;
                    for (int i = 0; i < viewCount; i++)
                    {
                        //Log.i("MyApp", "ord=id:" + viewOrder[i] + "==" + cv.getId());
                        if (viewOrder[i] == currentView.Id)
                        {
                            curIdx = i;
                            break;
                        }
                    }

                    if (curIdx >= 0)
                    {
                        preIdx = curIdx - 1;
                        nxtIdx = curIdx + 1;
                        preIdx = (preIdx < 0) ? viewCount - 1 : preIdx;
                        nxtIdx = (nxtIdx >= viewCount) ? 0 : nxtIdx;

                        prevView = this.FindViewById(viewOrder[preIdx]);
                        nextView = this.FindViewById(viewOrder[nxtIdx]);
                    }

                    Log.Info(LOG_TAG,
                             String.Format("Pre=%d(%d),Cur=%d(%d),Nxt=%d(%d)"
                                           , preIdx, prevView.Id
                                           , curIdx, currentView.Id
                                           , nxtIdx, nextView.Id));

                    break;
                case MotionEventActions.Move:
                    int travelDistanceX = (int) (currentX - this.startX);
                    int fingerPosX = (int) currentX;

                    if (flipMode == FLIPMODE_NOMOVE)
                    {
                        if (travelDistanceX > SWITCH_THRESHOLD)
                        {
                            flipMode = FLIPMODE_PREV;
                        }
                        else if
                            (travelDistanceX < (SWITCH_THRESHOLD*-1))
                        {
                            flipMode = FLIPMODE_NEXT;
                        }
                        else
                        {
                            flipMode = FLIPMODE_NOMOVE;
                        }
                    }

                    if (flipMode == FLIPMODE_PREV)
                    {
                        currentView.Layout(fingerPosX,
                                           currentView.Top,
                                           fingerPosX + currentView.Width,
                                           currentView.Bottom);

                        this.BringChildToFront(currentView);
                        prevView.Visibility = ViewStates.Visible;

                    }
                    if (flipMode == FLIPMODE_NEXT)
                    {
                        currentView.Layout(fingerPosX - currentView.Width,
                                           currentView.Top,
                                           fingerPosX,
                                           currentView.Bottom);

                        this.BringChildToFront(currentView);
                        nextView.Visibility = ViewStates.Visible;
                    }
                    break;

                case MotionEventActions.Up:

                    int activeIdx = -1;
                    if ((this.startX - currentX) > movePageThreshold)
                    {
                        activeIdx = nxtIdx;
                    }
                    else if
                        ((this.startX - currentX) < (movePageThreshold*-1))
                    {
                        activeIdx = preIdx;
                    }
                    else
                    {
                        activeIdx = curIdx;
                    }
                    int activeId = viewOrder[activeIdx];
                    for (int i = 0; i < this.ChildCount; i++)
                    {
                        // Log.i("MyApp",String.format("vf_id:%d,sel_id:%d",vf.getChildAt(i).getId(),activeId));
                        if (this.GetChildAt(i).Id == activeId)
                        {
                            this.DisplayedChild = i;
                            break;
                        }
                    }
                    flipMode = 0;
                    break;
                default:
                    break;
            }
            return true;
        }
    }
}