using System;
using UIKit;
using System.Windows.Input;
using CoreGraphics;
using System.Linq;

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

		private NSLayoutConstraint[] _hiddenContraints { get; set; }

		public bool HiddenWithConstraints
		{
			get
			{
				return base.Hidden;
			}
			set
			{
				if (base.Hidden != value)
				{
					base.Hidden = value;
					if (value)
					{
						_hiddenContraints = this.Superview.Constraints != null 
							? this.Superview.Constraints.Where(x => x.FirstItem == this || x.SecondItem == this).ToArray()
							: null;
						if (_hiddenContraints != null)
						{
							this.Superview.RemoveConstraints(_hiddenContraints);
						}
					}
					else
					{
						if (_hiddenContraints != null)
						{
							this.Superview.AddConstraints(_hiddenContraints);
							_hiddenContraints = null;
						}
					}
				}
			}
		}
    }
}

