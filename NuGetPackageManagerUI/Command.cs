using System;
using System.Diagnostics;
using System.Windows.Input;

namespace NuGetPackageManagerUI
{
	public class Command : ICommand
	{
		private readonly Action _action;

		public event EventHandler CanExecuteChanged;

		public Command(Action action)
		{
			if (action == null)
				throw new ArgumentNullException(nameof(action));

			_action = action;
		}

		public bool CanExecute(object parameter)
		{
			return true;
		}

		public void Execute(object parameter)
		{
			_action.Invoke();
		}
	}

	public class Command<T> : ICommand
	{
		private readonly Action<T> _action;

		public event EventHandler CanExecuteChanged;

		public Command(Action<T> action)
		{
			if (action == null)
				throw new ArgumentNullException(nameof(action));

			_action = action;
		}

		public bool CanExecute(object parameter)
		{
			return true;
		}

		public void Execute(object parameter)
		{ 
			Debug.Assert(parameter.GetType() == typeof(T), $"The command input parameters type is '{parameter.GetType()}', but the command parameters define type is '{typeof(T)}'");

			_action.Invoke((T)parameter);
		}
	}
}
