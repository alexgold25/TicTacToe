// TicTacToe.Core/Game.cs
using System;
using System.Linq;

namespace TicTacToe;

public class Game
{
    private static readonly int[][] _winningLines =
    {
        new[] {0,1,2},
        new[] {3,4,5},
        new[] {6,7,8},
        new[] {0,3,6},
        new[] {1,4,7},
        new[] {2,5,8},
        new[] {0,4,8},
        new[] {2,4,6}
    };

    public Cell[] Board { get; } = new Cell[9];
    public Cell CurrentPlayer { get; private set; } = Cell.X;
    public bool IsGameOver { get; private set; }
    public Cell Winner { get; private set; } = Cell.Empty;

    /// <summary>
    /// Делает ход в ячейку pos (0..8). Возвращает true, если ход принят.
    /// </summary>
    public bool MakeMove(int pos)
    {
        if (IsGameOver || pos < 0 || pos >= Board.Length) return false;
        if (Board[pos] != Cell.Empty) return false;

        Board[pos] = CurrentPlayer;

        if (CheckWinner(CurrentPlayer))
        {
            Winner = CurrentPlayer;
            IsGameOver = true;
        }
        else if (Board.All(c => c != Cell.Empty))
        {
            Winner = Cell.Empty;
            IsGameOver = true;
        }
        else
        {
            CurrentPlayer = CurrentPlayer == Cell.X ? Cell.O : Cell.X;
        }

        return true;
    }

    public void Reset()
    {
        for (int i = 0; i < Board.Length; i++)
            Board[i] = Cell.Empty;

        CurrentPlayer = Cell.X;
        Winner = Cell.Empty;
        IsGameOver = false;
    }

    private bool CheckWinner(Cell player)
    {
        foreach (var line in _winningLines)
        {
            if (Board[line[0]] == player &&
                Board[line[1]] == player &&
                Board[line[2]] == player)
                return true;
        }
        return false;
    }
}
