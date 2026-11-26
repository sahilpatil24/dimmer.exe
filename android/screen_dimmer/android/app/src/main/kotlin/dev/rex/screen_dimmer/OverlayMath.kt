package dev.rex.screen_dimmer

import android.graphics.Color
import kotlin.math.abs
import kotlin.math.roundToInt

/**
 * Turns a [DimmerConfig] into the single ARGB color drawn by the overlay.
 *
 * Two layers are composed into one: a color-filter tint, then black on top.
 *
 * Android 12+ blocks touches that pass through an overlay window whose
 * *window* alpha is above 0.8, so the window alpha is pinned to
 * [WINDOW_ALPHA] and the color alpha is scaled up to compensate. That means
 * the effective darkness can never exceed 0.8, and the limits below keep the
 * combined result under it (0.72 + 0.28 * (1 - 0.72) is about 0.79). It also
 * guarantees the screen never goes fully black.
 */
object OverlayMath {
    const val MAX_DIM_ALPHA = 0.72f
    const val MAX_TINT_ALPHA = 0.28f
    const val WINDOW_ALPHA = 0.8f

    private val WARM = intArrayOf(255, 138, 30)
    private val COOL = intArrayOf(70, 130, 255)

    /** Effective (on-screen) opacity of the combined overlay, 0..0.8. */
    fun effectiveAlpha(config: DimmerConfig): Float {
        val c = config.sanitized()
        val d = c.dimPercent / 100f * MAX_DIM_ALPHA
        val t = tintAlpha(c)
        return d + t * (1f - d)
    }

    fun overlayColor(config: DimmerConfig): Int {
        val c = config.sanitized()
        val d = c.dimPercent / 100f * MAX_DIM_ALPHA
        val t = tintAlpha(c)
        val total = d + t * (1f - d)
        if (total <= 0.0005f) return Color.TRANSPARENT

        val rgb = if (c.tintPercent >= 0) WARM else COOL
        // Black contributes no color, so only the tint layer is weighted.
        val tintWeight = t * (1f - d) / total
        val r = (rgb[0] * tintWeight).roundToInt()
        val g = (rgb[1] * tintWeight).roundToInt()
        val b = (rgb[2] * tintWeight).roundToInt()
        val a = (total / WINDOW_ALPHA).coerceIn(0f, 1f)
        return Color.argb((a * 255f).roundToInt(), r, g, b)
    }

    /** 0..1 window brightness override, or null to leave the system value. */
    fun panelBrightness(config: DimmerConfig): Float? {
        val c = config.sanitized()
        return if (c.panelLimitEnabled) c.panelLimitPercent / 100f else null
    }

    private fun tintAlpha(c: DimmerConfig): Float =
        if (c.filterEnabled) abs(c.tintPercent) / 100f * MAX_TINT_ALPHA else 0f
}
