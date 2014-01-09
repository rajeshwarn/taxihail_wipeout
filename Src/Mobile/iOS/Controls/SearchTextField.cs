using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    [Register("SearchTextField")]
    public class SearchTextField : TextField
    {

        private string _eventRaiseForText;
        private bool _hasDelayedChangeEvent;
        public SearchTextField(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        public SearchTextField(RectangleF rect) : base( rect )
        {
            Initialize();
        }

        private new void Initialize()
        {         

            EditingChanged += delegate
            {
                CancelMoveMap();
                
                _moveMapCommand = new CancellationTokenSource();
                
                var t = new Task(() => Thread.Sleep(700), _moveMapCommand.Token);
                
                t.ContinueWith(r =>
                {
                    if (r.IsCompleted && !r.IsCanceled && !r.IsFaulted)
                    {
						InvokeOnMainThread(OnTextChanged); 
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
            if (_eventRaiseForText != Text)
            {
                if (TextChangedCommand != null && TextChangedCommand.CanExecute())
                {
                    _eventRaiseForText = Text;
                    TextChangedCommand.Execute(Text);
                }
                else
                {
                    _hasDelayedChangeEvent = true;
                }
            }
        }

        private ICommand _textChangedCommand;

        public ICommand TextChangedCommand
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
                var hasChanged = false;
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