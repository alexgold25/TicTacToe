using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace TicTacToe;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly Game _game = new();
    private readonly RandomBot _bot = new();

    public ObservableCollection<string> BoardView { get; } = new(new string[9]);

    private string _status = string.Empty;
    public string Status
    {
        get => _status;
        set { _status = value; OnPropertyChanged(); }
    }

    private bool _playVsRandom;
    public bool PlayVsRandom
    {
        get => _playVsRandom;
        set { _playVsRandom = value; OnPropertyChanged(); OnPropertyChanged(nameof(ModeText)); }
    }

    public string ModeText => PlayVsRandom ? "Play vs Random" : "Play vs Human";

    public ICommand MakeMoveCommand { get; }
    public ICommand NewGameCommand { get; }
    public ICommand ToggleModeCommand { get; }

    public MainViewModel()
    {
        MakeMoveCommand = new RelayCommand(p => MakeMove((int)p!), p => CanMakeMove((int)p!));
        NewGameCommand = new RelayCommand(_ => NewGame());
        ToggleModeCommand = new RelayCommand(_ => ToggleMode());
        UpdateBoard();
        UpdateStatus();
    }

    private bool CanMakeMove(int index) => !_game.IsGameOver && _game.Board[index] == Cell.Empty;

    private void MakeMove(int index)
    {
        if (!_game.MakeMove(index)) return;
        UpdateBoard();
        UpdateStatus();

        if (PlayVsRandom && !_game.IsGameOver && _game.CurrentPlayer == Cell.O)
        {
            var move = _bot.GetMove(_game.Board);
            if (move >= 0)
            {
                _game.MakeMove(move);
                UpdateBoard();
                UpdateStatus();
            }
        }

        System.Windows.Input.CommandManager.InvalidateRequerySuggested();
    }

    private void NewGame()
    {
        _game.Reset();
        UpdateBoard();
        UpdateStatus();
        System.Windows.Input.CommandManager.InvalidateRequerySuggested();
    }

    private void ToggleMode()
    {
        PlayVsRandom = !PlayVsRandom;
        NewGame();
    }

    private void UpdateBoard()
    {
        for (int i = 0; i < BoardView.Count; i++)
        {
            BoardView[i] = _game.Board[i] switch
            {
                Cell.X => "X",
                Cell.O => "O",
                _ => string.Empty
            };
        }
    }

    private void UpdateStatus()
    {
        if (_game.IsGameOver)
        {
            Status = _game.Winner switch
            {
                Cell.X => "Winner: X",
                Cell.O => "Winner: O",
                _ => "Draw"
            };
        }
        else
        {
            Status = $"Turn: {_game.CurrentPlayer}";
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
