// --------------------------------------------------------------------------------
// <copyright file="RelayCommand.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AccuSync.Presentation.Helpers
{
    /// <summary>
    /// Minimal async-aware <see cref="ICommand"/> implementation. CanExecute is
    /// re-evaluated only when <see cref="RaiseCanExecuteChanged"/> is called explicitly —
    /// callers must invoke it whenever a value their CanExecute depends on changes.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Func<Task> _executeAsync;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Func<Task> executeAsync, Func<bool> canExecute = null)
        {
            _executeAsync = executeAsync;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute();

        // NOTE: async void is unavoidable here — ICommand.Execute returns void.
        // Exceptions will surface as unobserved task exceptions unless the Func
        // passed to the constructor has its own try/catch (which both ViewModels do).
        public async void Execute(object parameter) => await _executeAsync();

        public event EventHandler CanExecuteChanged;

        /// <summary>Raises <see cref="CanExecuteChanged"/> — call whenever a value <see cref="CanExecute"/> depends on changes.</summary>
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
