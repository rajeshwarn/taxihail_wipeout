using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Extensions;
using Cirrious.MvvmCross.Interfaces.Commands;

namespace apcurium.MK.Booking.Mobile
{
    public class AsyncCommand : IMvxCommand, IDisposable
    {
        private Func<bool> _canExecute;
        private Action _execute;
        private bool _isExecuting;

        public AsyncCommand(Action execute)
            : this(execute, null)
        {
        }

        public AsyncCommand(Action execute, Func<bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || ( !_isExecuting && _canExecute());
        }

        public bool CanExecute()
        {
            return CanExecute(null);
        }

        public void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                _isExecuting = true;
                OnCanExecuteChanged();
                Task.Factory.StartNew(() => _execute()).HandleErrors().ContinueWith(_ =>
                                                                                        {
                                                                                            _isExecuting = false;
                                                                                            OnCanExecuteChanged();
                                                                                        });
            }
        }

        public void Execute()
        {
            Execute(null);
        }

        public event EventHandler CanExecuteChanged;

        protected virtual void OnCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, new EventArgs());
            }
        }

        #region IDisposable implementation

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _isExecuting = false;
                _execute = null;
                _canExecute = null;
            }
        }

        #endregion

    }

    public class AsyncCommand<T> : IMvxCommand, IDisposable
        {
        private Func<T,bool> _canExecute;
        private Action<T> _execute;
        private bool _isExecuting;

        public AsyncCommand(Action<T> execute)
            : this(execute, null)
        {
        }

        public AsyncCommand(Action<T> execute, Func<T,bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || (!_isExecuting && _canExecute((T)parameter));
        }

        public bool CanExecute()
        {
            return CanExecute(null);
        }

        public void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                _isExecuting = true;
                OnCanExecuteChanged();
                    Task.Factory.StartNew(() => _execute((T)parameter))
					.HandleErrors()
					.ContinueWith(_ =>
                    {
                        _isExecuting = false;
                        OnCanExecuteChanged();
                    });
            }
        }

        public void Execute()
        {
            Execute(null);
        }

        public event EventHandler CanExecuteChanged;

        protected virtual void OnCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, new EventArgs());
            }
        }

        #region IDisposable implementation

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _isExecuting = false;
                _execute = null;
                _canExecute = null;
            }
        }

        #endregion
    }

}