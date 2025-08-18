package com.alexgold25.tictactoe

enum class Cell { E, X, O }

class Game {
    val board: Array<Cell> = Array(9) { Cell.E }
    var current: Cell = Cell.X;     private set
    var winner: Cell = Cell.E;      private set
    var isOver: Boolean = false;    private set

    private val lines = arrayOf(
        intArrayOf(0,1,2), intArrayOf(3,4,5), intArrayOf(6,7,8),
        intArrayOf(0,3,6), intArrayOf(1,4,7), intArrayOf(2,5,8),
        intArrayOf(0,4,8), intArrayOf(2,4,6)
    )

    fun reset() {
        for (i in board.indices) board[i] = Cell.E
        current = Cell.X; winner = Cell.E; isOver = false
    }

    fun makeMove(pos: Int): Boolean {
        if (isOver || pos !in 0..8 || board[pos] != Cell.E) return false
        board[pos] = current
        isOver = checkWinner(current) || board.none { it == Cell.E }
        winner = if (isOver && checkWinner(current)) current else if (isOver) Cell.E else Cell.E
        if (!isOver) current = if (current == Cell.X) Cell.O else Cell.X
        return true
    }

    fun winningLine(): IntArray? {
        if (!isOver || winner == Cell.E) return null
        for (l in lines) {
            val (a,b,c) = l
            if (board[a]==winner && board[b]==winner && board[c]==winner) return l
        }
        return null
    }

    private fun checkWinner(p: Cell): Boolean {
        for (l in lines) {
            val (a,b,c) = l
            if (board[a]==p && board[b]==p && board[c]==p) return true
        }
        return false
    }
}
