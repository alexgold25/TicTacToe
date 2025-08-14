using TicTacToe; // из твоего Core

namespace TicTacToe.Blazor.Services;

public class GameService
{
    public Game Game { get; } = new();
    private readonly RandomBot _bot = new();

    public bool PlayVsRandom { get; set; }
    public bool SoundEnabled { get; set; } = true;

    public string[] View { get; } = new string[9];
    public bool[] Highlight { get; } = new bool[9];

    private int _scoreX, _scoreO, _draws;

    public string ScoresText => $"Score X:{_scoreX} O:{_scoreO} D:{_draws}";
    public string TurnText =>
        Game.IsGameOver
            ? (Game.Winner == Cell.Empty ? "Draw" : $"Winner: {Game.Winner}")
            : $"{Game.CurrentPlayer}";

    public event Action? Changed;
    private void Notify() => Changed?.Invoke();

    public GameService()
    {
        UpdateView();
    }

    public void NewGame()
    {
        Game.Reset();
        Array.Fill(Highlight, false);
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

    private void UpdateWinVisuals()
    {
        Array.Fill(Highlight, false);
        if (!Game.IsGameOver || Game.Winner == Cell.Empty) return;

        foreach (var c in WinCombos)
        {
            var a = c[0]; var b = c[1]; var d = c[2];
            var cellA = Game.Board[a];
            if (cellA != Cell.Empty &&
                cellA == Game.Board[b] &&
                cellA == Game.Board[d])
            {
                Highlight[a] = Highlight[b] = Highlight[d] = true;
                break;
            }
        }
    }

    private static void TryPlayWin()
    {
        // В WASM без JS-аудио оставим пусто (можно добавить IJSRuntime и проигрывать короткий звук)
    }
}
