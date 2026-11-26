package dev.rex.screen_dimmer

import android.content.Context

/**
 * Persists [DimmerConfig] and the "dimmer should be active" flag.
 * Native code is the source of truth so the service, tile and boot receiver
 * work even when the Flutter engine isn't running.
 */
class ConfigStore(context: Context) {
    private val prefs =
        context.applicationContext.getSharedPreferences(PREFS, Context.MODE_PRIVATE)

    var config: DimmerConfig
        get() {
            val d = DimmerConfig()
            return DimmerConfig(
                dimPercent = prefs.getInt(K_DIM, d.dimPercent),
                filterEnabled = prefs.getBoolean(K_FILTER, d.filterEnabled),
                tintPercent = prefs.getInt(K_TINT, d.tintPercent),
                panelLimitEnabled = prefs.getBoolean(K_PANEL_ON, d.panelLimitEnabled),
                panelLimitPercent = prefs.getInt(K_PANEL, d.panelLimitPercent),
                resumeOnBoot = prefs.getBoolean(K_BOOT, d.resumeOnBoot),
            ).sanitized()
        }
        set(value) {
            val c = value.sanitized()
            prefs.edit()
                .putInt(K_DIM, c.dimPercent)
                .putBoolean(K_FILTER, c.filterEnabled)
                .putInt(K_TINT, c.tintPercent)
                .putBoolean(K_PANEL_ON, c.panelLimitEnabled)
                .putInt(K_PANEL, c.panelLimitPercent)
                .putBoolean(K_BOOT, c.resumeOnBoot)
                .apply()
        }

    /** True from the moment the user turns the dimmer on until they turn it off. */
    var active: Boolean
        get() = prefs.getBoolean(K_ACTIVE, false)
        set(value) {
            prefs.edit().putBoolean(K_ACTIVE, value).apply()
        }

    private companion object {
        const val PREFS = "dimmer_prefs"
        const val K_DIM = "dim"
        const val K_FILTER = "filter_enabled"
        const val K_TINT = "tint"
        const val K_PANEL_ON = "panel_limit_enabled"
        const val K_PANEL = "panel_limit"
        const val K_BOOT = "resume_on_boot"
        const val K_ACTIVE = "active"
    }
}
