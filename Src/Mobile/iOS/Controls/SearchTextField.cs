using System;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using MonoTouch.CoreAnimation;
using MonoTouch.CoreFoundation;
using MonoTouch.Foundation;
using System.Drawing;
using Cirrious.MvvmCross.Interfaces.Commands;
using System.Threading;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.Client
{
    [Register("SearchTextField")]
    public class SearchTextField : TextField
    {

        private string _eventRaiseForText;
        private bool _hasDelayedChangeEvent = false;

        public SearchTextField(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        public SearchTextField(RectangleF rect) : base( rect )
        {
            Initialize();
        }

        private void Initialize()
        {         

            this.EditingChanged += delegate
            {
                CancelMoveMap();
                
                _moveMapCommand = new CancellationTokenSource();
                
                var t = new Task(() =>
                {
                    Thread.Sleep(700);
                }, _moveMapCommand.Token);
                
                t.ContinueWith(r =>
                {
                    if (r.IsCompleted && !r.IsCanceled && !r.IsFaulted)
                    {
                        OnTextChanged();  
                    }        
                }, _moveMapCommand.Token);
                t.Start();
            };

        }

        private CancellationTokenSource _moveMapCommand;

        void CancelMoveMap()
        {
            if ((_moveMapCommand != null) && _moveMapCommand.Token.CanBeCanceled)
            {
                _moveMapCommand.Cancel();
                _moveMapCommand.Dispose();
                _moveMapCommand = null;
            }
        }

        void OnTextChanged()
        {
            if (_eventRaiseForText != this.Text)
            {
                if (TextChangedCommand != null && TextChangedCommand.CanExecute())
                {
                    _eventRaiseForText = Text;
                    TextChangedCommand.Execute(this.Text);
                }
                else
                {
                    _hasDelayedChangeEvent = true;
                }
            }
        }

        private IMvxCommand _textChangedCommand;

        public IMvxCommand TextChangedCommand
        { 
            get{ return _textChangedCommand;}
            set
            {
                _textChangedCommand = value;
                if ((_textChangedCommand != null) && _hasDelayedChangeEvent)
                {
                    _hasDelayedChangeEvent = false;
                    OnTextChanged();
                }
            }
        }

        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                bool hasChanged = false;
                if (base.Text != value)
                {
                    hasChanged = true;
                }

                base.Text = value;

                if (hasChanged)
                {
                    OnTextChanged();
                }
            }
        }
    }
}