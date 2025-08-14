using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Media;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace TicTacToe;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly Game _game = new();
    private readonly RandomBot _bot = new();

    public ObservableCollection<string> BoardView { get; } = new(new string[9]);
    public ObservableCollection<bool> Highlight { get; } = new(new bool[9]); // winning flags per cell

    // win line overlay (for base 360x360 board with 10 px padding)
    private const double BaseSize = 360.0;
    private const double Pad = 10.0;
    private static double CellSize => (BaseSize - 2 * Pad) / 3.0;

    private double _winX1, _winY1, _winX2, _winY2;
    public double WinX1 { get => _winX1; set { _winX1 = value; OnPropertyChanged(); } }
    public double WinY1 { get => _winY1; set { _winY1 = value; OnPropertyChanged(); } }
    public double WinX2 { get => _winX2; set { _winX2 = value; OnPropertyChanged(); } }
    public double WinY2 { get => _winY2; set { _winY2 = value; OnPropertyChanged(); } }

    private bool _winLineVisible;
    public bool WinLineVisible { get => _winLineVisible; set { _winLineVisible = value; OnPropertyChanged(); } }

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
        set
        {
            if (_playVsRandom == value) return;
            _playVsRandom = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ModeText));
            NewGame(); // restart when mode flips
        }
    }

    public string ModeText => PlayVsRandom ? "Vs Computer" : "Vs Human";

    private bool _soundEnabled = true;
    public bool SoundEnabled
    {
        get => _soundEnabled;
        set { _soundEnabled = value; OnPropertyChanged(); }
    }

    // Scores
    private int _scoreX, _scoreO, _draws;
    public string ScoresText => $"Score X:{_scoreX} O:{_scoreO} D:{_draws}";

    public ICommand MakeMoveCommand { get; }
    public ICommand NewGameCommand { get; }
    public ICommand SetHumanCommand { get; }
    public ICommand SetComputerCommand { get; }
    public ICommand ResetScoresCommand { get; }

    public MainViewModel()
    {
        MakeMoveCommand = new RelayCommand(
            p => { if (TryGetIndex(p, out var i)) MakeMove(i); },
            p => TryGetIndex(p, out var i) && CanMakeMove(i));

        NewGameCommand = new RelayCommand(_ => NewGame());
        SetHumanCommand = new RelayCommand(_ => PlayVsRandom = false);
        SetComputerCommand = new RelayCommand(_ => PlayVsRandom = true);
        ResetScoresCommand = new RelayCommand(_ => ResetScores());

        UpdateBoard();
        UpdateStatus();
        ClearHighlight();
        HideLine();
        NotifyScores();
    }

    private bool CanMakeMove(int index) => !_game.IsGameOver && _game.Board[index] == Cell.Empty;

    private void MakeMove(int index)
    {
        if (!_game.MakeMove(index)) return;
        PlayClick();
        UpdateBoard();
        UpdateStatus();
        UpdateWinVisuals();

        if (_game.IsGameOver)
        {
            ApplyScore();
            PlayWin();
            InvalidateCommands();
            return;
        }

        if (PlayVsRandom && _game.CurrentPlayer == Cell.O)
        {
            var move = _bot.GetMove(_game.Board);
            if (move >= 0)
            {
                _game.MakeMove(move);
                PlayClick();
                UpdateBoard();
                UpdateStatus();
                UpdateWinVisuals();

                if (_game.IsGameOver)
                {
                    ApplyScore();
                    PlayWin();
                }
            }
        }

        InvalidateCommands();
    }

    private void NewGame()
    {
        _game.Reset();
        UpdateBoard();
        UpdateStatus();
        ClearHighlight();
        HideLine();
        InvalidateCommands();
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

    // winning visuals: highlights + line
    private static readonly int[][] WinCombos =
    {
        new[] {0,1,2}, new[] {3,4,5}, new[] {6,7,8},
        new[] {0,3,6}, new[] {1,4,7}, new[] {2,5,8},
        new[] {0,4,8}, new[] {2,4,6}
    };

    private void UpdateWinVisuals()
    {
        ClearHighlight();
        HideLine();

        if (!_game.IsGameOver) return;

        if (_game.Winner == Cell.Empty)
            return; // draw

        foreach (var combo in WinCombos)
        {
            int a = combo[0], b = combo[1], c = combo[2];
            var cellA = _game.Board[a];
            if (cellA != Cell.Empty &&
                cellA == _game.Board[b] &&
                cellA == _game.Board[c])
            {
                // highlight cells
                Highlight[a] = Highlight[b] = Highlight[c] = true;

                // line from center(a) to center(c) (works for rows, cols, diagonals)
                var (x1, y1) = CenterOf(a);
                var (x2, y2) = CenterOf(c);
                WinX1 = x1; WinY1 = y1; WinX2 = x2; WinY2 = y2;
                WinLineVisible = true;
                break;
            }
        }
    }

    private static (double x, double y) CenterOf(int index)
    {
        int row = index / 3;
        int col = index % 3;
        double x = Pad + CellSize * (col + 0.5);
        double y = Pad + CellSize * (row + 0.5);
        return (x, y);
    }

    private void HideLine() => WinLineVisible = false;

    private void ClearHighlight()
    {
        for (int i = 0; i < Highlight.Count; i++)
            Highlight[i] = false;
    }

    private void ApplyScore()
    {
        switch (_game.Winner)
        {
            case Cell.X: _scoreX++; break;
            case Cell.O: _scoreO++; break;
            default: _draws++; break;
        }
        NotifyScores();
    }

    private void ResetScores()
    {
        _scoreX = _scoreO = _draws = 0;
        NotifyScores();
    }

    private void NotifyScores() => OnPropertyChanged(nameof(ScoresText));

    private static bool TryGetIndex(object? parameter, out int index)
    {
        if (parameter is null) { index = -1; return false; }
        return int.TryParse(parameter.ToString(), out index);
    }

    private void InvalidateCommands() => CommandManager.InvalidateRequerySuggested();

    // sounds
    private void PlayClick()
    {
        if (!SoundEnabled) return;
        SystemSounds.Asterisk.Play();
    }

    private void PlayWin()
    {
        if (!SoundEnabled) return;
        SystemSounds.Exclamation.Play();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
