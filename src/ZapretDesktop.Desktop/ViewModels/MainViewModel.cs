using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZapretDesktop.Core;
using ZapretDesktop.Core.Models;
using ZapretDesktop.Core.Services;

namespace ZapretDesktop.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ProcessManager _processManager;
    private readonly ListManager _listManager;
    private readonly PresetManager _presetManager;
    private readonly AutoStrategyTester _autoStrategyTester;

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private bool _isAutoTesting;

    [ObservableProperty]
    private string _statusText = "Служба остановлена";

    [ObservableProperty]
    private string _logs = string.Empty;

    [ObservableProperty]
    private ZapretConfig _config = new();

    [ObservableProperty]
    private Preset? _selectedPreset;

    public ObservableCollection<string> AvailableLists { get; } = [];
    public ObservableCollection<string> SelectedLists { get; } = [];
    public ObservableCollection<Preset> Presets { get; } = [];

    public MainViewModel()
    {
        _processManager = new ProcessManager();
        _listManager = new ListManager();
        _presetManager = new PresetManager();
        _autoStrategyTester = new AutoStrategyTester(_processManager);

        _processManager.LogReceived += OnLogReceived;
        _processManager.ProcessExited += OnProcessExited;
        _autoStrategyTester.TestProgressLog += OnLogReceived;

        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        RefreshLists();
        var loadedPresets = await _presetManager.LoadPresetsAsync();
        
        App.Current.Dispatcher.Invoke(() =>
        {
            Presets.Clear();
            foreach (var p in loadedPresets) Presets.Add(p);
            SelectedPreset = Presets.FirstOrDefault();
        });
    }

    [RelayCommand]
    private void ToggleService()
    {
        if (IsRunning)
        {
            _processManager.Stop();
            IsRunning = false;
            StatusText = "Служба остановлена";
        }
        else
        {
            string listsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "lists");
            Config.SelectedLists = SelectedLists.ToList();

            string args = SelectedPreset != null 
                ? SelectedPreset.Arguments 
                : Config.BuildCommandLine(listsDir);

            if (_processManager.Start(args))
            {
                IsRunning = true;
                StatusText = $"Запущено ({SelectedPreset?.Name ?? "Кастомный конфиг"})";
            }
            else
            {
                StatusText = "Ошибка запуска winws.exe";
            }
        }
    }

    [RelayCommand]
    private async Task RunAutoTestAsync()
    {
        if (IsAutoTesting || IsRunning) return;

        IsAutoTesting = true;
        StatusText = "Тестирование стратегий...";

        var winningPreset = await _autoStrategyTester.TestStrategiesAsync(Presets);

        if (winningPreset != null)
        {
            SelectedPreset = winningPreset;
            StatusText = $"Найдена стратегия: {winningPreset.Name}";
            ToggleService();
        }
        else
        {
            StatusText = "Ни одна стратегия не подошла";
        }

        IsAutoTesting = false;
    }

    [RelayCommand]
    private async Task AddCustomListAsync()
    {
        var openFileDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
            Title = "Выберите файл списка доменов"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            string fileName = Path.GetFileName(openFileDialog.FileName);
            var lines = await File.ReadAllLinesAsync(openFileDialog.FileName);
            await _listManager.AddCustomListAsync(fileName, lines);
            RefreshLists();
        }
    }

    private void RefreshLists()
    {
        AvailableLists.Clear();
        foreach (var list in _listManager.GetAvailableLists())
        {
            AvailableLists.Add(list);
        }
    }

    private void OnLogReceived(string message)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            Logs += $"[{DateTime.Now:HH:mm:ss}] {message}\n";
        });
    }

    private void OnProcessExited(int exitCode)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            IsRunning = false;
            StatusText = $"Служба завершилась (код: {exitCode})";
        });
    }
}