using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZapretDesktop.Core.Engines;
using ZapretDesktop.Core.Models;
using ZapretDesktop.Core.Services;

namespace ZapretDesktop.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IZapretEngine _engine;
    private readonly ListManager _listManager;

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private string _statusText = "Служба остановлена";

    [ObservableProperty]
    private ObservableCollection<ZapretPreset> _presets = new();

    [ObservableProperty]
    private ZapretPreset? _selectedPreset;

    [ObservableProperty]
    private string _userListContent = string.Empty;

    public MainViewModel()
    {
        _engine = ZapretEngineFactory.Create();
        _listManager = new ListManager();

        Presets = new ObservableCollection<ZapretPreset>(ZapretPreset.GetDefaultPresets());
        SelectedPreset = Presets.FirstOrDefault();

        _ = LoadUserListAsync();
    }

    [RelayCommand]
    private async Task ToggleServiceAsync()
    {
        if (IsRunning)
        {
            _engine.Stop();
            IsRunning = false;
            StatusText = "Служба остановлена";
            return;
        }

        if (SelectedPreset == null)
        {
            MessageBox.Show("Выберите пресет перед запуском.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            if (!_engine.IsInstalled)
            {
                StatusText = "Загрузка winws...";
                await _engine.EnsureInstalledAsync();
            }

            _engine.Start(SelectedPreset.Arguments);
            IsRunning = true;
            StatusText = $"Запущено: {SelectedPreset.Name}";
        }
        catch (Exception ex)
        {
            IsRunning = false;
            StatusText = "Ошибка запуска";
            MessageBox.Show($"Не удалось запустить службу: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task SaveUserListAsync()
    {
        try
        {
            await _listManager.SaveListAsync("list-general-user.txt", UserListContent);
            MessageBox.Show("Список 'list-general-user.txt' сохранён!", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LoadUserListAsync()
    {
        UserListContent = await _listManager.ReadListAsync("list-general-user.txt");
    }
}