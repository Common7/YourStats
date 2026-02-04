using Avalonia.Threading;
using ReactiveUI;
using System.Windows.Input; // For ICommand
using System;                    // Fixes 'Action' and 'EventHandler'
using System.Windows.Input;      // Fixes 'ICommand'
using Avalonia.Threading;        // Fixes 'Dispatcher'

namespace YourStats.App.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private ViewModelBase _currentPage;

    public MainWindowViewModel()
    {
        _currentPage = new DashboardViewModel();

        // Use a simple Action-based command instead of ReactiveCommand
        ShowDashboard = new RelayCommand(() => SetPage(new DashboardViewModel()));
        ShowMatches = new RelayCommand(() => SetPage(new MatchesViewModel()));
        ShowStats = new RelayCommand(() => SetPage(new StatsViewModel()));
    }

    private void SetPage(ViewModelBase page)
    {
        // This ensures the actual UI swap happens on the UI thread
        Dispatcher.UIThread.Post(() => CurrentPage = page);
    }

    public ViewModelBase CurrentPage
    {
        get => _currentPage;
        set => this.RaiseAndSetIfChanged(ref _currentPage, value);
    }

    public ICommand ShowDashboard { get; }
    public ICommand ShowMatches { get; }
    public ICommand ShowStats { get; }
}

// Simple helper class to avoid thread-heavy ReactiveCommands for now
public class RelayCommand(Action execute) : ICommand
{
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => execute();
    public event EventHandler? CanExecuteChanged;
}