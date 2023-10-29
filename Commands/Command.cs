using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace kostka_rgb.Commands
{
    public class Command : ICommand
    {
        public Command(Action action) =>
            Action = action;

        public Action Action { get; }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) =>
            Action?.Invoke();
    }
}
