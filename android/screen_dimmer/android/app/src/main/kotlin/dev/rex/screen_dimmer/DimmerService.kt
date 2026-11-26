package dev.rex.screen_dimmer

import android.app.Notification
import android.app.NotificationChannel
import android.app.NotificationManager
import android.app.PendingIntent
import android.app.Service
import android.content.ComponentName
import android.content.Context
import android.content.Intent
import android.content.pm.ServiceInfo
import android.graphics.drawable.Icon
import android.os.Build
import android.os.IBinder
import android.service.quicksettings.TileService
import android.util.Log

/**
 * Foreground service that keeps the overlay alive while the user works in
 * other apps. The Flutter UI, quick tile and notification all control it.
 */
class DimmerService : Service() {
    private lateinit var store: ConfigStore
    private lateinit var overlay: OverlayController

    override fun onBind(intent: Intent?): IBinder? = null

    override fun onCreate() {
        super.onCreate()
        store = ConfigStore(this)
        overlay = OverlayController(this)
        instance = this
        ensureChannel()
    }

    override fun onStartCommand(intent: Intent?, flags: Int, startId: Int): Int {
        if (intent?.action == ACTION_STOP) {
            shutdown()
            return START_NOT_STICKY
        }

        // Must happen promptly after startForegroundService().
        enterForeground()

        // System restarted us after a kill, but the user had turned it off.
        if (intent == null && !store.active) {
            shutdown()
            return START_NOT_STICKY
        }

        if (!overlay.show(store.config)) {
            // Permission revoked or window refused: don't pretend to be running.
            shutdown()
            return START_NOT_STICKY
        }

        store.active = true
        running = true
        requestTileRefresh()
        return START_STICKY
    }

    /** Re-reads the saved config and pushes it to the live overlay. */
    fun refresh() {
        overlay.update(store.config)
    }

    /** Stops dimming, removes the notification and ends the service. */
    fun shutdown() {
        store.active = false
        overlay.hide()
        running = false
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.N) {
            stopForeground(STOP_FOREGROUND_REMOVE)
        } else {
            @Suppress("DEPRECATION")
            stopForeground(true)
        }
        stopSelf()
        requestTileRefresh()
    }

    override fun onDestroy() {
        overlay.hide()
        running = false
        instance = null
        requestTileRefresh()
        super.onDestroy()
    }

    private fun enterForeground() {
        val notification = buildNotification()
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.UPSIDE_DOWN_CAKE) {
            startForeground(
                NOTIFICATION_ID,
                notification,
                ServiceInfo.FOREGROUND_SERVICE_TYPE_SPECIAL_USE,
            )
        } else {
            startForeground(NOTIFICATION_ID, notification)
        }
    }

    private fun ensureChannel() {
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.O) return
        val manager = getSystemService(NotificationManager::class.java)
        val channel = NotificationChannel(
            CHANNEL_ID,
            "Dimmer status",
            NotificationManager.IMPORTANCE_LOW,
        ).apply {
            description = "Shown while the screen dimmer is active"
            setShowBadge(false)
        }
        manager.createNotificationChannel(channel)
    }

    private fun buildNotification(): Notification {
        val builder = if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            Notification.Builder(this, CHANNEL_ID)
        } else {
            @Suppress("DEPRECATION")
            Notification.Builder(this)
        }

        val openApp = packageManager.getLaunchIntentForPackage(packageName)
        if (openApp != null) {
            builder.setContentIntent(
                PendingIntent.getActivity(
                    this,
                    0,
                    openApp,
                    PendingIntent.FLAG_IMMUTABLE or PendingIntent.FLAG_UPDATE_CURRENT,
                ),
            )
        }

        val stopIntent = Intent(this, DimmerService::class.java).setAction(ACTION_STOP)
        val stopPending = PendingIntent.getService(
            this,
            1,
            stopIntent,
            PendingIntent.FLAG_IMMUTABLE or PendingIntent.FLAG_UPDATE_CURRENT,
        )
        val stopAction = Notification.Action.Builder(
            Icon.createWithResource(this, R.drawable.ic_dimmer),
            "Turn off",
            stopPending,
        ).build()

        return builder
            .setSmallIcon(R.drawable.ic_dimmer)
            .setContentTitle("Screen Dimmer is on")
            .setContentText("Tap to adjust, or turn it off here")
            .setCategory(Notification.CATEGORY_SERVICE)
            .setOngoing(true)
            .setOnlyAlertOnce(true)
            .addAction(stopAction)
            .build()
    }

    private fun requestTileRefresh() {
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.N) return
        try {
            // String name on purpose: avoids a compile-time cycle with the tile.
            TileService.requestListeningState(
                this,
                ComponentName(this, "dev.rex.screen_dimmer.DimmerTileService"),
            )
        } catch (e: Exception) {
            Log.w(TAG, "Could not refresh quick tile", e)
        }
    }

    companion object {
        const val ACTION_STOP = "dev.rex.screen_dimmer.action.STOP"
        private const val TAG = "DimmerService"
        private const val CHANNEL_ID = "dimmer_overlay"
        private const val NOTIFICATION_ID = 1

        @Volatile
        var running: Boolean = false
            private set

        @Volatile
        var instance: DimmerService? = null
            private set

        fun start(context: Context) {
            val intent = Intent(context, DimmerService::class.java)
            try {
                if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
                    context.startForegroundService(intent)
                } else {
                    context.startService(intent)
                }
            } catch (e: Exception) {
                Log.w(TAG, "Could not start dimmer service", e)
            }
        }

        fun stop(context: Context) {
            ConfigStore(context).active = false
            instance?.shutdown()
        }
    }
}
