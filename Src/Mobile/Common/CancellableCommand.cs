using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using apcurium.MK.Common.Diagnostic;
using Cirrious.CrossCore;

namespace apcurium.MK.Booking.Mobile
{
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