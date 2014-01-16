using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.CrossCore;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile
{
    public class AsyncCommand : ICommand, IDisposable
    {
		readonly ILogger _logger;
        private Func<bool> _canExecute;
        private Action _execute;
        private bool _isExecuting;

        public AsyncCommand(Action execute)
            : this(execute, null)
        {
        }

        public AsyncCommand(Action execute, Func<bool> canExecute)
        {
			_logger = Mvx.Resolve<ILogger>();
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

		public async void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                _isExecuting = true;
                OnCanExecuteChanged();
				try
				{
					await Task.Factory.StartNew(() => _execute(),
						default(CancellationToken),
						TaskCreationOptions.None,
						GetTaskScheduler());
				}
				catch(Exception e)
				{
					_logger.LogError(e);
				}
				finally
				{
					_isExecuting = false;
					OnCanExecuteChanged();
				}
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

		private TaskScheduler GetTaskScheduler()
		{
			try
			{
				return TaskScheduler.FromCurrentSynchronizationContext();
			}
			catch(Exception e)
			{
				_logger.LogError(e);
				return TaskScheduler.Default;
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

    public class AsyncCommand<T> : ICommand, IDisposable
    {
		readonly ILogger _logger;
        private Func<T,bool> _canExecute;
        private Action<T> _execute;
        private bool _isExecuting;

        public AsyncCommand(Action<T> execute)
            : this(execute, null)
        {
        }

        public AsyncCommand(Action<T> execute, Func<T,bool> canExecute)
        {
			_logger = Mvx.Resolve<ILogger>();
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

		public async void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                _isExecuting = true;
                OnCanExecuteChanged();

				try
				{
					await Task.Factory.StartNew(() => _execute((T)parameter),
						default(CancellationToken),
						TaskCreationOptions.None,
						GetTaskScheduler()
					);
				}
				catch(Exception e)
				{
					_logger.LogError(e);
				}
				finally
				{
					_isExecuting = false;
					OnCanExecuteChanged();
				}
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

		private TaskScheduler GetTaskScheduler()
		{
			try
			{
				return TaskScheduler.FromCurrentSynchronizationContext();
			}
			catch(Exception e)
			{
				_logger.LogError(e);
				return TaskScheduler.Default;
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