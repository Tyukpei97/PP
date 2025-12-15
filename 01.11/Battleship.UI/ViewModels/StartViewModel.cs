using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Battleship.UI.Services;
using Battleship.UI.ViewModels.Base;

namespace Battleship.UI.ViewModels;

public class StartViewModel : ViewModelBase
{
    private readonly GameCoordinator _coordinator;
    private readonly Action _onStarted;
    private string _nickname = "Игрок";
    private string _ip = "127.0.0.1";
    private int _port = 5000;
    private bool _isBusy;
    private string _status = "Введите данные и выберите режим";

    public string Nickname
    {
        get => _nickname;
        set => SetField(ref _nickname, value);
    }

    public string IpAddress
    {
        get => _ip;
        set => SetField(ref _ip, value);
    }

    public int Port
    {
        get => _port;
        set => SetField(ref _port, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetField(ref _isBusy, value);
    }

    public string Status
    {
        get => _status;
        set => SetField(ref _status, value);
    }

    public ICommand StartHostCommand { get; }
    public ICommand StartClientCommand { get; }

    public StartViewModel(GameCoordinator coordinator, Action onStarted)
    {
        _coordinator = coordinator;
        _onStarted = onStarted;
        StartHostCommand = new RelayCommand(async _ => await StartHostAsync(), _ => !IsBusy);
        StartClientCommand = new RelayCommand(async _ => await StartClientAsync(), _ => !IsBusy);
    }

    private async Task StartHostAsync()
    {
        IsBusy = true;
        Status = "Запуск сервера...";
        try
        {
            await _coordinator.StartHostAsync(Port, Nickname);
            Status = "Сервер создан. Размещайте корабли.";
            _onStarted();
        }
        catch (Exception ex)
        {
            Status = $"Ошибка: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task StartClientAsync()
    {
        IsBusy = true;
        Status = "Подключение к серверу...";
        try
        {
            await _coordinator.StartClientAsync(IpAddress, Port, Nickname);
            Status = "Соединение установлено. Размещайте корабли.";
            _onStarted();
        }
        catch (Exception ex)
        {
            Status = $"Ошибка: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
