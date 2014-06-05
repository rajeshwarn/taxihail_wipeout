using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using apcurium.MK.Common.Diagnostic;
using Cirrious.CrossCore;

namespace apcurium.MK.Booking.Mobile
{
    public class AsyncCommand : ICommand, IDisposable
    {
		readonly ILogger _logger;
        private Func<bool> _canExecute;
		private Func<Task> _execute;
        private bool _isExecuting;

        public AsyncCommand(Action execute)
			: this(execute, null)
        {
        }

		public AsyncCommand(Func<Task> execute)
			: this(execute, null)
		{
		}

        public AsyncCommand(Action execute, Func<bool> canExecute)
			: this(Wrap(execute), canExecute)
        {
        }

		public AsyncCommand(Func<Task> execute, Func<bool> canExecute)
		{
			_logger = Mvx.Resolve<ILogger>();
			_execute = execute;
			_canExecute = canExecute;
		}

        public bool CanExecute(object parameter)
        {
			if (_isExecuting)
			{
				return false;
			}

			if (_canExecute == null)
			{
				return true;
			}

			return _canExecute();
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
                    var t = _execute();
                    await t;
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

		public void RaiseCanExecuteChanged()
		{
			OnCanExecuteChanged ();
		}

        private static TaskScheduler GetTaskScheduler()
		{
			try
			{
				return TaskScheduler.FromCurrentSynchronizationContext();
			}
			catch(Exception)
			{
				return TaskScheduler.Default;
			}
		}

        private static Func<Task> Wrap(Action execute)
		{
            return () =>
            {
                return Task.Factory.StartNew(execute,
                    default(CancellationToken),
                    TaskCreationOptions.None,
                    GetTaskScheduler());
            };
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
		private Func<T, Task> _execute;
        private bool _isExecuting;

        public AsyncCommand(Action<T> execute)
            : this(execute, null)
        {
        }

		public AsyncCommand(Func<T, Task> execute)
			: this(execute, null)
		{
		}

        public AsyncCommand(Action<T> execute, Func<T,bool> canExecute)
			: this(Wrap(execute), canExecute)
        {
        }

        public AsyncCommand(Func<T, Task> execute, Func<T, bool> canExecute)
		{
			_logger = Mvx.Resolve<ILogger>();
			_execute = execute;
			_canExecute = canExecute;
		}

        public bool CanExecute(object parameter)
        {
			if (_isExecuting)
			{
				return false;
			}

			if (_canExecute == null)
			{
				return true;
			}

			return _canExecute((T)parameter);
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
                    var t = _execute((T)parameter);
					await t;
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

        private static TaskScheduler GetTaskScheduler()
		{
			try
			{
				return TaskScheduler.FromCurrentSynchronizationContext();
			}
			catch(Exception)
			{
				return TaskScheduler.Default;
			}
		}

        private static Func<T, Task> Wrap(Action<T> execute)
		{
			return p =>
			{
				return Task.Factory.StartNew(() => execute(p),
					default(CancellationToken),
					TaskCreationOptions.None,
					GetTaskScheduler());
			};
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