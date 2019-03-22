using System;
using System.Windows.Input;

namespace CHAD.DesktopApplication.Tools
{
    public class RelayCommand<T> : ICommand
    {
        #region Fields

        private readonly Predicate<T> _canExecute;

        private readonly Action<T> _execute;

        #endregion

        #region Constructors

        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            _execute = execute;
            _canExecute = canExecute;
        }

        #endregion

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke((T) parameter) ?? true;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public void Execute(object parameter)
        {
            _execute((T) parameter);
        }

        #endregion
    }
}