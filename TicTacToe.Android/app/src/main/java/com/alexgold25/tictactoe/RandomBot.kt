package com.alexgold25.tictactoe

import kotlin.random.Random

class RandomBot {
    private val lines = arrayOf(
        intArrayOf(0,1,2), intArrayOf(3,4,5), intArrayOf(6,7,8),
        intArrayOf(0,3,6), intArrayOf(1,4,7), intArrayOf(2,5,8),
        intArrayOf(0,4,8), intArrayOf(2,4,6)
    )
    private val rnd = Random

    fun nextMove(board: Array<Cell>, me: Cell): Int {
        findFinish(board, me)?.let { return it }
        val opp = if (me == Cell.X) Cell.O else Cell.X
        findFinish(board, opp)?.let { return it }
        val empty = board.mapIndexedNotNull { i, c -> if (c==Cell.E) i else null }
        return empty.random(rnd)
    }

    private fun findFinish(b: Array<Cell>, p: Cell): Int? {
        for (l in lines) {
            val e = l.firstOrNull { b[it]==Cell.E } ?: continue
            if (l.count { b[it]==p }==2 && l.count { b[it]==Cell.E }==1) return e
        }
        return null
    }
}
