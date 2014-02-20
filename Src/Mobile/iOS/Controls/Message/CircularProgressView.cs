using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.CoreAnimation;
using MonoTouch.Foundation;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using MonoTouch.CoreGraphics;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Message
{
    public class CircularProgressView : UIView
    {
        private CAShapeLayer _progressBackgroundLayer;
        private CAShapeLayer _progressLayer;
        private CAShapeLayer _iconLayer;

        public CircularProgressView(RectangleF frame, UIColor color) : base(frame)
        {
            _runningIconView = new UIView();
            _readyIconView = new UIView();
            _completedIconView = new UIView();

            BackgroundColor = UIColor.Clear;

            _lineWidth = Math.Max(Frame.Width * .025f, 1f);

            _progressBackgroundLayer = new CAShapeLayer();
            _progressBackgroundLayer.FillColor = BackgroundColor.CGColor;   
            _progressBackgroundLayer.LineCap = CAShapeLayer.CapRound;
            _progressBackgroundLayer.LineWidth = _lineWidth;

            Layer.AddSublayer(_progressBackgroundLayer);

            _progressLayer = new CAShapeLayer();
            _progressLayer.FillColor = null;
            _progressLayer.LineCap = CAShapeLayer.CapSquare;
            _progressLayer.LineWidth = _lineWidth * 2.0f;
            Layer.AddSublayer(_progressLayer);

            _iconLayer = new CAShapeLayer();
            _iconLayer.FillColor = null;
            _iconLayer.LineCap = CAShapeLayer.CapButt;
            _iconLayer.LineWidth = _lineWidth;
            _iconLayer.FillRule = CAShapeLayer.FillRuleNonZero;
            Layer.AddSublayer(_iconLayer);      

            Color = color;
        }

        float _lineWidth;
        public float LineWidth
        {
            get
            {
                return _lineWidth;
            }
            set
            {           
                _lineWidth = Math.Max(value, 1f);

                _progressBackgroundLayer.LineWidth = _lineWidth;
                _progressLayer.LineWidth = _lineWidth * 2f;
                _iconLayer.LineWidth = _lineWidth;
            }
        }

        UIColor _color;
        public UIColor Color
        {
            get
            {
                return _color;
            }
            set
            {
                _color = value;

                _progressLayer.StrokeColor = value.CGColor;
                _iconLayer.StrokeColor = value.CGColor;
            }
        }

        float _progress;
        public float Progress
        {
            get
            {
                return _progress;
            }
            set
            {
                if (value > 1.0f) value = 1.0f;

                if (_progress != value)
                {
                    _progress = value;

                    if (_progress == 0f) {
                        _progressBackgroundLayer.FillColor = BackgroundColor.CGColor;
                    }

                    SetNeedsDisplay();
                }
            }
        }

        async void ProgressCompleted()
        {
            await Task.Delay(TimeSpan.FromSeconds(0.5));
            OnCompleted.Invoke();
        }

        public Action OnCompleted { get; set; }

        UIView _readyIconView;
        public UIView ReadyIconView 
        { 
            get 
            { 
                return _readyIconView; 
            } 
            set 
            {
                _readyIconView.RemoveFromSuperview();
                _readyIconView = value; 
                AddNewIcon(value);
            } 
        }

        UIView _runningIconView;
        public UIView RunningIconView 
        { 
            get 
            { 
                return _runningIconView; 
            } 
            set 
            {
                _runningIconView.RemoveFromSuperview();
                _runningIconView = value; 
                AddNewIcon(value);      
            } 
        }

        UIView _completedIconView;
        public UIView CompletedIconView 
        { 
            get 
            { 
                return _completedIconView; 
            } 
            set 
            {
                _completedIconView.RemoveFromSuperview();
                _completedIconView = value; 
                AddNewIcon(value);
            } 
        }

        void OnReady()
        {
            ReadyIconView.Hidden = false;
            RunningIconView.Hidden = true;
            CompletedIconView.Hidden = true;
        }

        void OnRunning()
        {
            ReadyIconView.Hidden = true;
            RunningIconView.Hidden = false;
            CompletedIconView.Hidden = true;
        }

        void OnComplete()
        {
            ReadyIconView.Hidden = true;
            RunningIconView.Hidden = true;
            CompletedIconView.Hidden = false;

            ProgressCompleted();
        }

        void AddNewIcon(UIView value)
        {
            Add(value);
            var left = (Bounds.Width - value.Bounds.Width)/2f;
            var top = (Bounds.Height - value.Bounds.Height)/2f;

            value
                .SetX(left)
                .SetY(top);
        }

        public override void Draw(RectangleF rect)
        {
            base.Draw(rect);
            _progressBackgroundLayer.Frame = Bounds;
            _progressLayer.Frame = Bounds;
            _iconLayer.Frame = Bounds;

            var center = new PointF(Bounds.Width/2, Bounds.Height/2);

            // Draw progress
            var startAngle = - ((float)Math.PI / 2); // 90 degrees
            var endAngle = (Progress * 2 * (float)Math.PI) + startAngle;
            var processPath = new UIBezierPath();
            processPath.LineCapStyle = MonoTouch.CoreGraphics.CGLineCap.Butt;
            processPath.LineWidth = _lineWidth;

            var radius = (Bounds.Width - _lineWidth * 3f) / 2.0f;

            processPath.AddArc(center, radius, startAngle, endAngle, true);

            _progressLayer.Path = processPath.CGPath;

            if (Progress == 1.0) 
            {
                OnComplete();
            }
            else if (Progress > 0) 
            {
                OnRunning();
            }
            else
            {
                OnReady();
            }
        }
    }
}

