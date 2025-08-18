package com.alexgold25.tictactoe

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.viewModels
import androidx.compose.animation.animateColorAsState
import androidx.compose.foundation.Canvas
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.runtime.remember
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.StrokeCap
import androidx.compose.ui.graphics.drawscope.Stroke
import androidx.compose.ui.platform.LocalConfiguration
import androidx.compose.ui.platform.LocalDensity
import androidx.compose.ui.text.TextStyle
import androidx.compose.ui.text.font.FontFamily
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.Dp
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.compose.ui.draw.drawBehind
import androidx.compose.foundation.border
import androidx.compose.ui.draw.clip

// ----- Цвета/радиусы ---------------------------------------------------------

private val Bg         = Color(0xFF0F1115)
private val CardBg     = Color(0xFF161A20)
private val CardActive = Color(0xFF1E2430)
private val Accent     = Color(0xFF3AAFFF)
private val AccentSoft = Color(0x593AAFFF)  // акцент с альфой для свечения
private val TextSoft   = Color(0xFFBFE6FF)
private val Corner     = 16.dp
private val Border     = Color(0xFF3A424C)  // было темнее; сделали контрастнее
private val BorderGlow = Accent            // акцент для победных


// ----- Типографика (строго, аккуратно) ---------------------------------------

private val AppTypography = Typography(
    titleLarge = TextStyle(
        fontFamily = FontFamily.SansSerif,
        fontWeight = FontWeight.SemiBold,
        fontSize = 22.sp,
        letterSpacing = 0.2.sp,
        color = TextSoft
    ),
    labelLarge = TextStyle(
        fontFamily = FontFamily.SansSerif,
        fontWeight = FontWeight.SemiBold,
        fontSize = 14.sp,
        color = TextSoft
    ),
    bodyLarge = TextStyle(
        fontFamily = FontFamily.SansSerif,
        fontWeight = FontWeight.Medium,
        fontSize = 16.sp,
        color = TextSoft
    )
)

class MainActivity : ComponentActivity() {
    private val vm: GameViewModel by viewModels()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContent {
            MaterialTheme(
                colorScheme = darkColorScheme(),
                typography = AppTypography
            ) {
                Surface(Modifier.fillMaxSize(), color = Bg) {
                    TicTacToeScreen(vm)
                }
            }
        }
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun TicTacToeScreen(vm: GameViewModel) {
    Scaffold(
        topBar = {
            CenterAlignedTopAppBar(
                title = { Text("TicTacToe", style = MaterialTheme.typography.titleLarge) },
                colors = TopAppBarDefaults.centerAlignedTopAppBarColors(containerColor = Bg)
            )
        },
        containerColor = Bg
    ) { padding ->
        Column(
            Modifier
                .fillMaxSize()
                .padding(padding)
                .padding(horizontal = 16.dp, vertical = 8.dp),
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            // 1) строка с кнопками
            Row(
                Modifier.fillMaxWidth(),
                verticalAlignment = Alignment.CenterVertically,
                horizontalArrangement = Arrangement.spacedBy(10.dp)
            ) {
                Button(
                    onClick = { vm.newGame() },
                    shape = RoundedCornerShape(Corner),
                    colors = ButtonDefaults.buttonColors(containerColor = Accent, contentColor = Color.Black)
                ) { Text("New Game", style = MaterialTheme.typography.labelLarge) }

                OutlinedButton(
                    onClick = { vm.playVsBot = !vm.playVsBot },
                    shape = RoundedCornerShape(Corner),
                    border = ButtonDefaults.outlinedButtonBorder.copy(width = 1.dp),
                    colors = ButtonDefaults.outlinedButtonColors(contentColor = TextSoft)
                ) {
                    Text(if (vm.playVsBot) "Vs Computer" else "Vs Human", style = MaterialTheme.typography.labelLarge)
                }
            }

            // 2) строка со счётом — отдельная и справа
            Row(
                Modifier
                    .fillMaxWidth()
                    .padding(top = 8.dp),
                horizontalArrangement = Arrangement.End
            ) {
                ScoreChip(x = vm.xWins, o = vm.oWins, d = vm.draws)
            }

            Spacer(Modifier.height(10.dp))

            // Адаптивный размер поля (до 360dp)
            val boardSize = rememberWindowInfo().minSideDp * 0.8f
            val size = boardSize.coerceAtMost(360f).dp

            BoardGrid(
                board = vm.board,
                isOver = vm.isOver,
                winningCombo = vm.winningCombo,
                onCellClick = { vm.put(it) },
                boardSize = size       // ← было: size = size
            )
            Spacer(Modifier.height(12.dp))

            Text(
                text = when {
                    vm.isOver && vm.winner == Cell.E -> "Draw"
                    vm.isOver -> "Winner: ${vm.winner}"
                    else -> "Your move"
                },
                style = MaterialTheme.typography.bodyLarge
            )
        }
    }
}

// ----- Компоненты -------------------------------------------------------------

@Composable
fun ScoreChip(x: Int, o: Int, d: Int) {
    Surface(
        color = CardBg,
        contentColor = TextSoft,
        shape = RoundedCornerShape(999.dp)
    ) {
        Row(
            Modifier.padding(horizontal = 12.dp, vertical = 6.dp),
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.spacedBy(10.dp)
        ) {
            Text("X: $x", style = MaterialTheme.typography.labelLarge, maxLines = 1, softWrap = false)
            Text("O: $o", style = MaterialTheme.typography.labelLarge, maxLines = 1, softWrap = false)
            Text("D: $d", style = MaterialTheme.typography.labelLarge, maxLines = 1, softWrap = false)
        }
    }
}

@Composable
fun BoardGrid(
    board: Array<Cell>,
    isOver: Boolean,
    winningCombo: IntArray?,
    onCellClick: (Int) -> Unit,
    boardSize: Dp                   // ← переименовали
) {
    Box(Modifier.size(boardSize)) {

        // — тонкие линии сетки под кнопками (видно даже на автояркости)
        Canvas(Modifier.matchParentSize().padding(6.dp)) {
            val gap = 12.dp.toPx()  // 6dp + 6dp
            val w = this.size.width
            val h = this.size.height

            val innerW = w - 2 * gap
            val innerH = h - 2 * gap
            val cellW = innerW / 3f
            val cellH = innerH / 3f

            val gridColor = Border.copy(alpha = 0.85f)
            val gridStroke = 1.25.dp.toPx()

            // вертикальные
            drawLine(gridColor, Offset(gap + cellW, 0f),           Offset(gap + cellW, h),           gridStroke)
            drawLine(gridColor, Offset(gap + 2*cellW + gap, 0f),   Offset(gap + 2*cellW + gap, h),   gridStroke)
            // горизонтальные
            drawLine(gridColor, Offset(0f, gap + cellH),           Offset(w,  gap + cellH),          gridStroke)
            drawLine(gridColor, Offset(0f, gap + 2*cellH + gap),   Offset(w,  gap + 2*cellH + gap),  gridStroke)
        }

        // — кнопки
        Column(Modifier.fillMaxSize()) {
            repeat(3) { r ->
                Row(Modifier.weight(1f).fillMaxWidth()) {
                    repeat(3) { c ->
                        val i = r * 3 + c
                        val label = when (board[i]) { Cell.X -> "X"; Cell.O -> "O"; else -> "" }
                        val active = winningCombo?.contains(i) == true
                        CellButton(
                            label = label,
                            highlight = active,
                            enabled = !isOver && board[i] == Cell.E,
                            onClick = { onCellClick(i) },
                            sizeHint = boardSize,                 // ← передаём новый параметр
                            modifier = Modifier
                                .weight(1f)
                                .fillMaxHeight()
                                .padding(6.dp)
                        )
                    }
                }
            }
        }

        // — линия победы поверх
        winningCombo?.let { WinLineOverlay(boardSize, it) }
    }
}



@Composable
fun CellButton(
    label: String,
    highlight: Boolean,
    enabled: Boolean,
    onClick: () -> Unit,
    sizeHint: Dp,
    modifier: Modifier = Modifier
) {
    val bg = animateColorAsState(if (highlight) CardActive else CardBg, label = "cell-bg").value
    val labelSize = (sizeHint.value / 8f).sp

    val strokeW = if (highlight) 2.25.dp else 1.5.dp
    val strokeColor = if (highlight) BorderGlow else Border
    val shape = RoundedCornerShape(12.dp)

    ElevatedButton(
        onClick = onClick,
        enabled = enabled,
        shape = shape,
        colors = ButtonDefaults.elevatedButtonColors(
            containerColor = bg,
            contentColor = Color(0xFFE7EDF5),
            disabledContainerColor = bg,
            disabledContentColor = Color(0x80E7EDF5)
        ),
        elevation = ButtonDefaults.elevatedButtonElevation(2.dp, 1.dp),
        contentPadding = PaddingValues(0.dp),
        modifier = modifier
            .clip(shape) // чтобы border лёг ровно по радиусу
            .border(strokeW, strokeColor, shape)
    ) {
        Text(label, fontSize = labelSize, fontWeight = FontWeight.Black, letterSpacing = 0.5.sp)
    }
}

/**
 * Победная линия «ядро + свечение» строго по центрам клеток.
 * Учитывает те же отступы, что и у кнопок (6dp у каждой клетки → 12dp зазор между соседями).
 */
@Composable
fun WinLineOverlay(boardSize: Dp, combo: IntArray) {
    val core = 10.dp
    val glow = 16.dp
    val outerPadding = 6.dp       // такой же, как padding у CellButton
    val gapBetweenCells = 12.dp   // 6dp слева у одной клетки + 6dp справа у соседней

    Canvas(Modifier.size(boardSize).padding(outerPadding)) {
        val gap = gapBetweenCells.toPx()

        // внутренняя область: две щели по горизонтали и вертикали
        val innerW = size.width  - 2 * gap
        val innerH = size.height - 2 * gap
        val cellW = innerW / 3f
        val cellH = innerH / 3f

        fun center(idx: Int): Offset {
            val col = idx % 3
            val row = idx / 3
            val x = col * (cellW + gap) + cellW / 2f
            val y = row * (cellH + gap) + cellH / 2f
            return Offset(x, y)
        }

        val a = center(combo.first())
        val b = center(combo.last())

        // мягкое свечение
        drawLine(
            color = AccentSoft, start = a, end = b,
            strokeWidth = glow.toPx(), cap = StrokeCap.Round
        )
        // «ядро»
        drawLine(
            color = TextSoft, start = a, end = b,
            strokeWidth = core.toPx(), cap = StrokeCap.Round
        )
    }
}

// ----- Утилита для адаптивного размера поля ----------------------------------

@Composable
fun rememberWindowInfo(): WindowInfo {
    val density = LocalDensity.current
    val cfg = LocalConfiguration.current
    val minSideDp = with(density) { minOf(cfg.screenWidthDp, cfg.screenHeightDp).toFloat() }
    return remember(cfg) { WindowInfo(minSideDp) }
}
data class WindowInfo(val minSideDp: Float)
