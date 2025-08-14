using Microsoft.JSInterop;
using TicTacToe; // из твоего Core

namespace TicTacToe.Blazor.Services;

public class GameService
{
    public Game Game { get; } = new();
    private readonly RandomBot _bot = new();
    private readonly IJSRuntime _js;

    public bool PlayVsRandom { get; set; }
    public bool SoundEnabled { get; set; } = true;

    public string[] View { get; } = new string[9];
    public bool[] Highlight { get; } = new bool[9];
    public Line? WinLine { get; private set; }

    private int _scoreX, _scoreO, _draws;

    public string ScoresText => $"Score X:{_scoreX} O:{_scoreO} D:{_draws}";
    public string TurnText =>
        Game.IsGameOver
            ? (Game.Winner == Cell.Empty ? "Draw" : $"Winner: {Game.Winner}")
            : $"{Game.CurrentPlayer}";

    public event Action? Changed;
    private void Notify() => Changed?.Invoke();

    public GameService(IJSRuntime js)
    {
        _js = js;
        UpdateView();
    }

    public void NewGame()
    {
        Game.Reset();
        Array.Fill(Highlight, false);
        WinLine = null;
        UpdateView();
        Notify();
    }

    public void MakeMove(int index)
    {
        if (index < 0 || index > 8) return;

        // ход игрока
        if (!Game.MakeMove(index)) return;

        UpdateWinVisuals();
        UpdateView();
        Notify();

        // если игра завершилась после хода игрока
        if (Game.IsGameOver)
        {
            ApplyScore();
            if (SoundEnabled) TryPlayWin();
            Notify();
            return;
        }

        // ход бота по необходимости
        if (PlayVsRandom && Game.CurrentPlayer == Cell.O)
        {
            var m = _bot.GetMove(Game.Board);
            if (m >= 0 && Game.MakeMove(m))
            {
                UpdateWinVisuals();
                UpdateView();

                if (Game.IsGameOver)
                {
                    ApplyScore();
                    if (SoundEnabled) TryPlayWin();
                }
            }
        }

        // единое уведомление об обновлении UI
        Notify();
    }

    private void ApplyScore()
    {
        switch (Game.Winner)
        {
            case Cell.X: _scoreX++; break;
            case Cell.O: _scoreO++; break;
            default: _draws++; break;
        }
    }

    private void UpdateView()
    {
        for (int i = 0; i < 9; i++)
        {
            View[i] = Game.Board[i] switch
            {
                Cell.X => "X",
                Cell.O => "O",
                _ => string.Empty
            };
        }
    }

    private static readonly int[][] WinCombos =
    {
        new[] {0,1,2}, new[] {3,4,5}, new[] {6,7,8},
        new[] {0,3,6}, new[] {1,4,7}, new[] {2,5,8},
        new[] {0,4,8}, new[] {2,4,6}
    };

    private static readonly Line[] WinLines =
    {
        new(0.1,0.5,2.9,0.5),
        new(0.1,1.5,2.9,1.5),
        new(0.1,2.5,2.9,2.5),
        new(0.5,0.1,0.5,2.9),
        new(1.5,0.1,1.5,2.9),
        new(2.5,0.1,2.5,2.9),
        new(0.1,0.1,2.9,2.9),
        new(2.9,0.1,0.1,2.9)
    };

    private void UpdateWinVisuals()
    {
        Array.Fill(Highlight, false);
        WinLine = null;
        if (!Game.IsGameOver || Game.Winner == Cell.Empty) return;

        for (int i = 0; i < WinCombos.Length; i++)
        {
            var c = WinCombos[i];
            var a = c[0]; var b = c[1]; var d = c[2];
            var cellA = Game.Board[a];
            if (cellA != Cell.Empty &&
                cellA == Game.Board[b] &&
                cellA == Game.Board[d])
            {
                Highlight[a] = Highlight[b] = Highlight[d] = true;
                WinLine = WinLines[i];
                break;
            }
        }
    }

    private async void TryPlayWin()
    {
        try
        {
            await _js.InvokeVoidAsync("eval", "new Audio('win.wav').play()");
        }
        catch
        {
            // ignore errors in sound playback
        }
    }
}

public record struct Line(double X1, double Y1, double X2, double Y2);

