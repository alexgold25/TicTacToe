using System;
using System.Collections.Generic;
using System.Linq;

namespace TicTacToe;

public class RandomBot
{
    private readonly Random _random = new();

    public int GetMove(Cell[] board)
    {
        var empty = new List<int>();
        for (int i = 0; i < board.Length; i++)
        {
            if (board[i] == Cell.Empty)
                empty.Add(i);
        }
        if (empty.Count == 0) return -1;
        return empty[_random.Next(empty.Count)];
    }
}
