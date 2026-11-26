package dev.rex.screen_dimmer

import android.content.Context
import android.graphics.PixelFormat
import android.os.Build
import android.provider.Settings
import android.util.Log
import android.view.Gravity
import android.view.View
import android.view.WindowManager

/** Owns the full-screen, non-touchable overlay window. */
class OverlayController(context: Context) {
    private val appContext = context.applicationContext
    private val windowManager =
        appContext.getSystemService(Context.WINDOW_SERVICE) as WindowManager

    private var view: View? = null
    private var params: WindowManager.LayoutParams? = null

    val isShowing: Boolean get() = view != null

    /** Adds the overlay if needed and applies [config]. Returns false on failure. */
    fun show(config: DimmerConfig): Boolean {
        if (!Settings.canDrawOverlays(appContext)) return false
        if (view == null) {
            val newView = View(appContext)
            val newParams = buildParams()
            try {
                windowManager.addView(newView, newParams)
            } catch (e: Exception) {
                Log.w(TAG, "Could not add overlay", e)
                return false
            }
            view = newView
            params = newParams
        }
        apply(config)
        return true
    }

    /** Applies [config] to a live overlay (no-op if it isn't showing). */
    fun update(config: DimmerConfig) {
        if (view == null) return
        apply(config)
    }

    fun hide() {
        val v = view ?: return
        try {
            windowManager.removeView(v)
        } catch (e: Exception) {
            Log.w(TAG, "Could not remove overlay", e)
        }
        view = null
        params = null
    }

    private fun apply(config: DimmerConfig) {
        val v = view ?: return
        val p = params ?: return

        v.setBackgroundColor(OverlayMath.overlayColor(config))

        val brightness = OverlayMath.panelBrightness(config)
            ?: WindowManager.LayoutParams.BRIGHTNESS_OVERRIDE_NONE
        if (p.screenBrightness != brightness) {
            p.screenBrightness = brightness
            try {
                windowManager.updateViewLayout(v, p)
            } catch (e: Exception) {
                Log.w(TAG, "Could not update overlay layout", e)
            }
        }
    }

    @Suppress("DEPRECATION")
    private fun buildParams(): WindowManager.LayoutParams {
        val type = if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            WindowManager.LayoutParams.TYPE_APPLICATION_OVERLAY
        } else {
            WindowManager.LayoutParams.TYPE_SYSTEM_OVERLAY
        }
        val flags = WindowManager.LayoutParams.FLAG_NOT_FOCUSABLE or
            WindowManager.LayoutParams.FLAG_NOT_TOUCHABLE or
            WindowManager.LayoutParams.FLAG_NOT_TOUCH_MODAL or
            WindowManager.LayoutParams.FLAG_LAYOUT_IN_SCREEN or
            WindowManager.LayoutParams.FLAG_LAYOUT_NO_LIMITS

        return WindowManager.LayoutParams(
            WindowManager.LayoutParams.MATCH_PARENT,
            WindowManager.LayoutParams.MATCH_PARENT,
            type,
            flags,
            PixelFormat.TRANSLUCENT,
        ).apply {
            gravity = Gravity.TOP or Gravity.START
            title = "ScreenDimmerOverlay"
            // Keep <= 0.8 so Android 12+ lets touches pass through. See OverlayMath.
            alpha = OverlayMath.WINDOW_ALPHA
            screenBrightness = WindowManager.LayoutParams.BRIGHTNESS_OVERRIDE_NONE
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R) {
                layoutInDisplayCutoutMode =
                    WindowManager.LayoutParams.LAYOUT_IN_DISPLAY_CUTOUT_MODE_ALWAYS
            } else if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.P) {
                layoutInDisplayCutoutMode =
                    WindowManager.LayoutParams.LAYOUT_IN_DISPLAY_CUTOUT_MODE_SHORT_EDGES
            }
        }
    }

    private companion object {
        const val TAG = "OverlayController"
    }
}
