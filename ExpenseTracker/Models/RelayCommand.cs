using System;

using System.Windows.Input;



namespace ExpenseTracker.ViewModels

{

    public class RelayCommand<T> : ICommand

    {

        private readonly Action<T> _execute;

        private readonly Func<T, bool>? _canExecute;

        private event EventHandler _canExecuteChanged;



        public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)

        {

            _execute = execute ?? throw new ArgumentNullException(nameof(execute));

            _canExecute = canExecute;

        }



        public bool CanExecute(object? parameter)

        {

            if (_canExecute == null)

                return true;



            if (parameter == null && typeof(T).IsValueType)

                return _canExecute(default!);



            if (parameter is T t)

                return _canExecute(t);



            return false;

        }



        public void Execute(object? parameter)

        {

            if (parameter is T t)

                _execute(t);

            else if (parameter == null)
                _execute(default!);

            else

                throw new InvalidCastException($"Invalid command parameter type. Expected {typeof(T)}, got {parameter?.GetType()}");

        }



        public event EventHandler? CanExecuteChanged

        {

            add

            {

                CommandManager.RequerySuggested += value;

                _canExecuteChanged += value!;

            }

            remove

            {

                CommandManager.RequerySuggested -= value;

                _canExecuteChanged -= value!;

            }

        }



        public void RaiseCanExecuteChanged()

        {

            CommandManager.InvalidateRequerySuggested();

        }

    }

}