package com.alexgold25.tictactoe

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel

class GameViewModel : ViewModel() {
    private val game = Game()
    private val bot = RandomBot()

    var board by mutableStateOf(game.board.copyOf());  private set
    var isOver by mutableStateOf(false);               private set
    var winner by mutableStateOf(Cell.E);              private set
    var winningCombo by mutableStateOf<IntArray?>(null); private set

    var playVsBot by mutableStateOf(true)
    var xWins by mutableStateOf(0); private set
    var oWins by mutableStateOf(0); private set
    var draws by mutableStateOf(0); private set

    fun newGame() { game.reset(); sync() }

    fun put(i: Int) {
        if (game.isOver) return
        if (!game.makeMove(i)) return
        sync()
        if (game.isOver) { score(); return }
        if (playVsBot && game.current==Cell.O && !game.isOver) {
            val m = bot.nextMove(game.board, Cell.O)
            game.makeMove(m); sync()
            if (game.isOver) score()
        }
    }

    private fun score() {
        when (game.winner) { Cell.X -> xWins++; Cell.O -> oWins++; Cell.E -> draws++ }
    }
    private fun sync() {
        board = game.board.copyOf()
        isOver = game.isOver
        winner = game.winner
        winningCombo = game.winningLine()
    }
}
