using System;
using System.Windows.Input;

namespace Devdeb.WPF
{
    public class Command : ICommand
    {
        private readonly Action<object> _executionCommand;
        private readonly Func<object, bool> _executionCommandPredicate;

        public Command(Action executionCommand) : this((parameter) => executionCommand()) { }
        public Command(Action executionCommand, Func<object, bool> executionCommandPredicate) : this((parameter) => executionCommand(), executionCommandPredicate) { }
        public Command(Action<object> executionCommand) : this(executionCommand, (parameter)=> true) { }
        public Command(Action<object> executionCommand, Func<object, bool> executionCommandPredicate)
        {
            _executionCommand = executionCommand;
            _executionCommandPredicate = executionCommandPredicate;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => _executionCommandPredicate(parameter);
        public void Execute(object parameter) => _executionCommand(parameter);

        protected virtual void CanExecute() => CanExecute(null);
        protected virtual void Execute() => Execute(null);
    }

    public class Command<TParameter> : Command 
    {
        public Command(Action<TParameter> executionCommand) : base((parameter) => executionCommand((TParameter)parameter)) { }
        public Command(Action<TParameter> executionCommand, Func<TParameter, bool> executionCommandPredicate) : base((parameter) => executionCommand((TParameter)parameter), (parameter)=> executionCommandPredicate((TParameter)parameter)) { }

        protected override void CanExecute() => CanExecute(default(TParameter));
        protected override void Execute() => Execute(default(TParameter));
    }
}