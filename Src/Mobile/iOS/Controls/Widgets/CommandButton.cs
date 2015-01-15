using System;
using UIKit;
using System.Windows.Input;
using CoreGraphics;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public class CommandButton : UIButton
    {
        public CommandButton(IntPtr handle) : base(handle)
        {
            Initialize ();
        }

        public CommandButton (CGRect frame) : base (frame)
        {
            Initialize ();
        }

        public CommandButton () : base ()
        {
            Initialize ();
        }

        private void Initialize()
        {
            TouchUpInside += HandleTouchUpInside;
        }

        private ICommand _command;
        public ICommand Command
        { 
            get { return _command; }
            set
            { 
                if (value != _command)
                {
                    var previous = _command;
                    _command = value;
                    OnCommandChanged(value, previous);
                }
            }
        }

        public object CommandParameter { get; set; }

        private void HandleTouchUpInside (object sender, EventArgs e)
        {
            Command.ExecuteIfPossible(CommandParameter);
        }

        private void HandleCanExecuteChanged(object sender, EventArgs e)
        {
            var command = (ICommand)sender;
            var canExecute = command.CanExecute(CommandParameter);
            this.Enabled = canExecute;
        }

        private void OnCommandChanged(ICommand newValue, ICommand oldValue)
        {
            if (oldValue != null)
            {
                oldValue.CanExecuteChanged -= HandleCanExecuteChanged;
            }
            if (newValue != null)
            {
                newValue.CanExecuteChanged += HandleCanExecuteChanged;
            }
        }
    }
}

