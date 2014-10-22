namespace System.Windows.Input
{
	public static class ICommandExtensions
    {

		public static void ExecuteIfPossible(this ICommand command, object parameter = null)
		{
			if (command != null && command.CanExecute(parameter))
			{
				command.Execute(parameter);
			}
		}

    }
}

