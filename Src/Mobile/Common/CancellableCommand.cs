using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using apcurium.MK.Common.Diagnostic;
using Cirrious.CrossCore;

namespace apcurium.MK.Booking.Mobile
{
	public class CancellableCommand : ICommand
	{
		private Func<bool> _canExecute;
		private Func<CancellationToken, Task> _execute;
		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		public CancellableCommand(Action<CancellationToken> execute)
			: this(execute, null)
		{
		}

		public CancellableCommand(Func<CancellationToken, Task> execute)
			: this(execute, null)
		{
		}

		public CancellableCommand(Action<CancellationToken> execute, Func<bool> canExecute)
			: this(Wrap(execute), canExecute)
		{
		}

		public CancellableCommand(Func<CancellationToken, Task> execute, Func<bool> canExecute)
		{
			_execute = execute;
			_canExecute = canExecute;
			new CancellationTokenSource().Dispose();
		}

		public bool CanExecute(object parameter)
		{
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
				var token = GetNewCancellationToken();
				try
				{
					await _execute(token);
				}
				catch(Exception)
				{
				}
			}
		}

		public void Execute()
		{
			Execute(null);
		}

		public void Cancel()
		{
			_cancellationTokenSource.Cancel();
		}

		public event EventHandler CanExecuteChanged;

		protected virtual void OnCanExecuteChanged()
		{
			if (CanExecuteChanged != null)
			{
				CanExecuteChanged(this, new EventArgs());
			}
		}

		private CancellationToken GetNewCancellationToken()
		{
			_cancellationTokenSource.Cancel();
			_cancellationTokenSource = new CancellationTokenSource();
			return _cancellationTokenSource.Token;
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

		private static Func<CancellationToken, Task> Wrap(Action<CancellationToken> execute)
		{
			return (token) =>
			{
				return Task.Factory.StartNew(() => execute(token),
					default(CancellationToken),
					TaskCreationOptions.None,
					GetTaskScheduler());
			};
		}
	}

	public class CancellableCommand<TParam> : ICommand
	{
		private Func<TParam,bool> _canExecute;
		private Func<TParam, CancellationToken, Task> _execute;
		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		public CancellableCommand(Func<TParam, CancellationToken, Task> execute, Func<TParam, bool> canExecute)
		{
			_execute = execute;
			_canExecute = canExecute;
			new CancellationTokenSource().Dispose();
		}

		public bool CanExecute(object parameter)
		{
			if (_canExecute == null)
			{
				return true;
			}

			return _canExecute((TParam)parameter);
		}

		public bool CanExecute()
		{
			return CanExecute(null);
		}

		public async void Execute(object parameter)
		{
			if (CanExecute(parameter))
			{
				var token = GetNewCancellationToken();
				try
				{
					await _execute((TParam)parameter, token);
				}
				catch(Exception)
				{
				}
			}
		}

		public void Execute()
		{
			Execute(null);
		}

		public void Cancel()
		{
			_cancellationTokenSource.Cancel();
		}

		public event EventHandler CanExecuteChanged;

		protected virtual void OnCanExecuteChanged()
		{
			if (CanExecuteChanged != null)
			{
				CanExecuteChanged(this, new EventArgs());
			}
		}

		private CancellationToken GetNewCancellationToken()
		{
			_cancellationTokenSource.Cancel();
			_cancellationTokenSource = new CancellationTokenSource();
			return _cancellationTokenSource.Token;
		}
	}
}