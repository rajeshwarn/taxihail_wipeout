//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using Android.App;
//using Android.Content;
//using Android.Graphics;
//using Android.Graphics.Drawables;
//using Android.Util;
//using Android.Views;
//using Android.Views.Animations;
//using Android.Widget;
//using apcurium.MK.Booking.Mobile.Client.Adapters;
//using apcurium.MK.Booking.Mobile.Client.Animations;
//using apcurium.MK.Booking.Mobile.Client.Models;

//namespace apcurium.MK.Booking.Mobile.Client.Controls
//{
//    public class IconActionControl : LinearLayout
//    {
//        private ListView _lv;
//        private readonly Animation _rotateRight = new RotateAnimation(0, 90, Dimension.RelativeToSelf, 0.5f, Dimension.RelativeToSelf, 0.5f);
//        readonly Animation _rotateLeft = new RotateAnimation(90, 0, Dimension.RelativeToSelf, 0.5f, Dimension.RelativeToSelf, 0.5f);
//        private ResizeAnimation _resizeDown;
//        private ResizeAnimation _resizeUp;
//        private ImageButton _button;
//        private FrameLayout _frameLayout;
//        public List<IconAction> ListIconAction { get; private set; }
//        private readonly Activity _activity;
//        private Drawable _bitmapUnselected;
//        private Drawable _bitmapSelected;
//        private readonly bool _toBottom;
//        private ImageView _endlineReverse;
//        private ImageView _endline;
//        private string _buttonIcon;

//        public IconActionControl(Activity activity, string buttonIcon, List<IconAction> listIconAction, bool toBottom)
//            : base(activity)
//        {
//            var inflater = (LayoutInflater)Context.ApplicationContext.GetSystemService(Context.LayoutInflaterService);
//            inflater.Inflate(Resource.Layout.IconActionLayout, this);
//            this.ListIconAction = listIconAction;
//            this._activity = activity;
//            this._toBottom = toBottom;
//            this._buttonIcon = buttonIcon;
//            Initialize();
//        }

//        private void Initialize()
//        {

//            _bitmapUnselected = Resources.GetDrawable(Resource.Drawable.ddc_list_btn_unselected);
//            _bitmapSelected = Resources.GetDrawable(Resource.Drawable.ddc_list_btn_selected);

//            _endlineReverse = FindViewById<ImageView>(Resource.Id.iconAction_endlineReverse);
//            _endlineReverse.SetImageDrawable(Resources.GetDrawable(Resource.Drawable.ddc_dropdownreverse_endline));

//            _endline = FindViewById<ImageView>(Resource.Id.iconAction_endline);
//            _endline.SetImageDrawable(Resources.GetDrawable(Resource.Drawable.ddc_dropdown_endline));

//            _button = FindViewById<ImageButton>(Resource.Id.iconAction_button);
//            _button.FocusChange += HandleFocusChange;

//            _frameLayout = FindViewById<FrameLayout>(Resource.Id.iconAction_frameLayout);
//            _frameLayout.SetBackgroundDrawable(_bitmapUnselected);

//            _lv = FindViewById<ListView>(_toBottom ? Resource.Id.iconAction_listViewBottom : Resource.Id.iconAction_listViewTop);

//            _lv.Adapter = new IconActionAdapter(_activity, Android.Resource.Layout.SimpleListItem1, ListIconAction);

//            _lv.SetBackgroundDrawable(Resources.GetDrawable(Resource.Drawable.ddc_dropdownreverse_bkgd));
//            this.InitializeAnimation();
//            _button.Click += new EventHandler(button_OnClick);
//            _button.SetBackgroundDrawable(null);
//            _button.SetImageBitmap(BitmapFactory.DecodeStream(Resources.Assets.Open(_buttonIcon)));
//            _lv.ItemClick += listView_itemSelected;
//            _lv.Divider = null;
//            _lv.DividerHeight = 0;
//            _lv.Visibility = ViewStates.Visible;
//        }

//        void HandleFocusChange (object sender, FocusChangeEventArgs e)
//        {
//            var ctl = sender as ImageButton;
//            if( !ctl.HasFocus && _lv.Height > 0 )
//            {
//                ResizeDown();
//            }       	
//        }

//        protected override void OnFocusChanged (bool gainFocus, FocusSearchDirection direction, Rect previouslyFocusedRect)
//        {
//            base.OnFocusChanged (gainFocus, direction, previouslyFocusedRect);
//        }

//        public void listView_itemSelected(object sender, AdapterView.ItemClickEventArgs e)
//        {
//            IconAction ia = ListIconAction.ElementAt(e.Position);
//            if ( ia.IntentAction != null )
//            {
//                try
//                {
//                    if( ia.IntentAction.Action != null && ia.IntentAction.Data == null )
//                    {
//                        _activity.SendBroadcast( ia.IntentAction );
//                    }
//                    else
//                    {
//                        if( ia.RequestCode.HasValue )
//                        {
//                            _activity.Parent.StartActivityForResult( ia.IntentAction, ia.RequestCode.Value );
//                        }
//                        else
//                        {
//                            _activity.StartActivity(ia.IntentAction);
//                        }
//                    }
//                    ResizeDown();
//                }
//                catch (Exception)
//                {
//                    LogPrinter lp = new LogPrinter(LogPriority.Error, "yop"); 
//                    lp.Println("Activite non valide");
//                }
//            }           
//        }

//        private void InitializeAnimation()
//        {
//            _rotateLeft.Duration = 250;
//            _rotateLeft.FillAfter = true;
//            _rotateRight.Duration = 250;
//            _rotateRight.FillAfter = true;
//            _resizeUp = new ResizeAnimation( _lv, 0, 61 * _lv.Count, true) { Duration = 500, Interpolator = new AccelerateDecelerateInterpolator()};
//            _resizeDown = new ResizeAnimation(_lv, 0, 61 * _lv.Count, false) { Duration = 500, Interpolator = new AccelerateDecelerateInterpolator() };
//            _resizeDown.AnimationEnd += ResizeDownOnAnimationEnd;
//            _resizeUp.AnimationEnd += ResizeUpAnimationEnd;
//        }

//        void ResizeUpAnimationEnd (object sender, Android.Views.Animations.Animation.AnimationEndEventArgs e)
//        {
//            _button.RequestFocus();
//        }

//        private void ResizeDownOnAnimationEnd(object sender, Animation.AnimationEndEventArgs animationEndEventArgs)
//        {
//            if (_toBottom)
//            {
//                _endline.Visibility = ViewStates.Invisible;
//            }
//            else
//            {
//                //_endlineReverse.Visibility = ViewStates.Invisible;
//            }

//        }

//        private void button_OnClick(object sender, EventArgs e)
//        {
//            if (_lv.Height.Equals(0))
//            {
//                if (_toBottom)
//                {
//                    _endline.Visibility = ViewStates.Visible;
//                }
//                else 
//                {
//                    //_endlineReverse.Visibility = ViewStates.Visible;
//                }

//                _frameLayout.SetBackgroundDrawable(_bitmapSelected);
//                _button.StartAnimation(_rotateRight);
//                _lv.StartAnimation(_resizeUp);
//            }
//            else
//            {
//                ResizeDown();
//            }
//        }

//        public void ResizeDown()
//        {
//            if (!_lv.Height.Equals(0))
//            {
//                _button.StartAnimation(_rotateLeft);
//                _lv.StartAnimation(_resizeDown);
//                _frameLayout.SetBackgroundDrawable(_bitmapUnselected);
//            }
//        }

//    }
//}

