package dev.rex.screen_dimmer

import android.app.PendingIntent
import android.content.Intent
import android.provider.Settings
import android.os.Build
import android.service.quicksettings.Tile
import android.service.quicksettings.TileService

/** Quick Settings tile: tap to toggle the dimmer from the notification shade. */
class DimmerTileService : TileService() {

    override fun onStartListening() {
        super.onStartListening()
        refreshTile()
    }

    override fun onClick() {
        super.onClick()
        if (DimmerService.running) {
            DimmerService.stop(this)
        } else if (Settings.canDrawOverlays(this)) {
            DimmerService.start(this)
        } else {
            openApp()
        }
        refreshTile()
    }

    private fun refreshTile() {
        val tile = qsTile ?: return
        tile.state = if (DimmerService.running) Tile.STATE_ACTIVE else Tile.STATE_INACTIVE
        tile.updateTile()
    }

    private fun openApp() {
        val launch = packageManager.getLaunchIntentForPackage(packageName) ?: return
        launch.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK)
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.UPSIDE_DOWN_CAKE) {
            startActivityAndCollapse(
                PendingIntent.getActivity(this, 0, launch, PendingIntent.FLAG_IMMUTABLE),
            )
        } else {
            @Suppress("DEPRECATION")
            startActivityAndCollapse(launch)
        }
    }
}
