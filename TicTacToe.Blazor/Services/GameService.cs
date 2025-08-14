using System;
using System.Linq;
using TicTacToe;

namespace TicTacToe.Blazor.Services;

public class GameService
{
    public Game Game { get; } = new();
    private readonly RandomBot _bot = new();

    public bool PlayVsRandom { get; set; } = false;
    public bool SoundEnabled { get; set; } = true;

    public int XWins { get; private set; }
    public int OWins { get; private set; }
    public int Draws { get; private set; }

    public string[] View { get; } = new string[9];
    public bool[] Highlight { get; } = new bool[9];
    public int[]? WinningCombo { get; private set; }

    public string DebugText { get; private set; } = string.Empty;

    public event Action? Changed;

    public void NewGame()
    {
        Game.Reset();
        Array.Fill(View, string.Empty);
        Array.Fill(Highlight, false);
        WinningCombo = null;
        Notify();
    }

    public void Put(int index)
    {
        if (Game.IsGameOver) return;             // стоп после конца
        if (index < 0 || index > 8) return;
        if (Game.Board[index] != Cell.Empty) return; // клик по занятой — игнор

        var ok = Game.MakeMove(index);           // ставим X/O
        UpdateWinVisuals();

        if (Game.IsGameOver)
        {
            ApplyScore();
            Notify();
            return;
        }

        // ход бота, если включен режим
        if (ok && PlayVsRandom && Game.CurrentPlayer == Cell.O && !Game.IsGameOver)
        {
            var m = _bot.GetMove(Game.Board);
            Game.MakeMove(m);
            UpdateWinVisuals();
            if (Game.IsGameOver) ApplyScore();
        }

        Notify();
    }


    private void ApplyScore()
    {
        if (!Game.IsGameOver) return;
        if (Game.Winner == Cell.X) XWins++;
        else if (Game.Winner == Cell.O) OWins++;
        else Draws++;
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

    private void UpdateWinVisuals()
    {
        Array.Fill(Highlight, false);
        WinningCombo = null;

        if (!Game.IsGameOver || Game.Winner == Cell.Empty) return;

        foreach (var line in new[]
        {
            new[] {0,1,2}, new[] {3,4,5}, new[] {6,7,8},
            new[] {0,3,6}, new[] {1,4,7}, new[] {2,5,8},
            new[] {0,4,8}, new[] {2,4,6}
        })
        {
            int a = line[0], b = line[1], c = line[2];
            if (Game.Board[a] != Cell.Empty && Game.Board[a] == Game.Board[b] && Game.Board[b] == Game.Board[c])
            {
                Highlight[a] = Highlight[b] = Highlight[c] = true;
                WinningCombo = line;
                break;
            }
        }
    }

    private void Notify() => Changed?.Invoke();
}
