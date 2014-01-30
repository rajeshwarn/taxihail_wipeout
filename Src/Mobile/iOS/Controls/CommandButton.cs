using System;
using MonoTouch.UIKit;
using System.Windows.Input;
using System.Drawing;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class CommandButton: UIButton
    {
        public CommandButton()
        {
            Initialize();
        }

        public CommandButton(IntPtr handle)
            :base(handle)
        {
            Initialize();
        }

        public CommandButton(UIButtonType type)
            :base(type)
        {
            Initialize();
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
            if (Command != null && Command.CanExecute(CommandParameter))
            {
                Command.Execute(CommandParameter);
            }
        }

        private void HandleCanExecuteChanged(object sender, EventArgs e)
        {
            var command = (ICommand)sender;
            bool canExecute = command.CanExecute(CommandParameter);
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

