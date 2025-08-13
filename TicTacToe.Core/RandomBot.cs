using System;
using System.Collections.Generic;
using System.Linq;

namespace TicTacToe;

public class RandomBot
{
    private readonly Random _random = new();

    private static readonly int[][] _lines = new int[][]
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

    public int GetMove(Cell[] board)
    {
        // Try to win
        var move = FindWinningMove(board, Cell.O);
        if (move >= 0) return move;

        // Block opponent
        move = FindWinningMove(board, Cell.X);
        if (move >= 0) return move;

        // Take center
        if (board[4] == Cell.Empty) return 4;

        // Take a corner if available
        var corners = new List<int> { 0, 2, 6, 8 }.Where(i => board[i] == Cell.Empty).ToList();
        if (corners.Count > 0) return corners[_random.Next(corners.Count)];

        // Fallback to any empty cell
        var empty = new List<int>();
        for (int i = 0; i < board.Length; i++)
        {
            if (board[i] == Cell.Empty)
                empty.Add(i);
        }
        if (empty.Count == 0) return -1;
        return empty[_random.Next(empty.Count)];
    }

    private static int FindWinningMove(Cell[] board, Cell player)
    {
        foreach (var line in _lines)
        {
            var cells = line.Select(i => board[i]).ToArray();
            if (cells.Count(c => c == player) == 2 && cells.Count(c => c == Cell.Empty) == 1)
            {
                return line.First(i => board[i] == Cell.Empty);
            }
        }
        return -1;
    }
}
