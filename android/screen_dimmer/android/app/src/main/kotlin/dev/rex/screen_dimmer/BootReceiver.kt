package dev.rex.screen_dimmer

import android.content.BroadcastReceiver
import android.content.Context
import android.content.Intent
import android.provider.Settings

/** Restores the dimmer after a reboot if the user asked for that. */
class BootReceiver : BroadcastReceiver() {
    override fun onReceive(context: Context, intent: Intent) {
        if (intent.action != Intent.ACTION_BOOT_COMPLETED) return
        val store = ConfigStore(context)
        if (store.active && store.config.resumeOnBoot && Settings.canDrawOverlays(context)) {
            DimmerService.start(context)
        }
    }
}
