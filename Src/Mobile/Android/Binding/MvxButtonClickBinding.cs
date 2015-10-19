using System;
using Android.Widget;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;
using Cirrious.MvvmCross.Binding.Droid.Target;
using Android.Views;
using System.Windows.Input;
using Cirrious.CrossCore.WeakSubscription;

namespace apcurium.MK.Booking.Mobile.Client.Binding
{

    /// <summary>
    /// Same as MvxViewClickBinding of MvvmCross but added an Alpha to add a disabled look
    /// </summary>
    public class MvxButtonClickBinding
        : MvxAndroidTargetBinding
    {
        private ICommand _command;
        private IDisposable _canExecuteSubscription;
        private readonly EventHandler<EventArgs> _canExecuteEventHandler;

        protected Button Button
        {
            get { return (Button)Target; }
        }

        public MvxButtonClickBinding(Button button)
            : base(button)
        {
            _canExecuteEventHandler = new EventHandler<EventArgs>(OnCanExecuteChanged);
            button.Click += ViewOnClick;
        }

        private void ViewOnClick(object sender, EventArgs args)
        {
            if (_command == null)
                return;

            if (!_command.CanExecute(null))
                return;

            _command.Execute(null);
        }

        protected override void SetValueImpl(object target, object value)
        {
            if (_canExecuteSubscription != null)
            {
                _canExecuteSubscription.Dispose();
                _canExecuteSubscription = null;
            }
            _command = value as ICommand;
            if (_command != null)
            {
                _canExecuteSubscription = _command.WeakSubscribe(_canExecuteEventHandler);
            }
            RefreshEnabledState();
        }

        private void RefreshEnabledState()
        {
            var button = Button;
            if (button == null)
                return;

            var shouldBeEnabled = false;
            if (_command != null)
            {
                shouldBeEnabled = _command.CanExecute(null);
            }

            button.Enabled = shouldBeEnabled;
            button.Alpha = shouldBeEnabled
                ? 1.0f
                : 0.5f;
        }

        private void OnCanExecuteChanged(object sender, EventArgs e)
        {
            RefreshEnabledState();
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.OneWay; }
        }

        public override Type TargetType
        {
            get { return typeof(ICommand); }
        }

        public static void Register(IMvxTargetBindingFactoryRegistry registry)
        {
            registry.RegisterFactory(new MvxCustomBindingFactory<Button>("Click", btn => new MvxButtonClickBinding(btn)));
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                var button = Button;
                if (button != null)
                {
                    button.Click -= ViewOnClick;
                }
                if (_canExecuteSubscription != null)
                {
                    _canExecuteSubscription.Dispose();
                    _canExecuteSubscription = null;
                }                
            }
            base.Dispose(isDisposing);
        }
    }
}