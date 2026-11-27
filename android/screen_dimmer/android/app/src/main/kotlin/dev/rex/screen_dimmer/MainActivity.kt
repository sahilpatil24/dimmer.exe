package dev.rex.screen_dimmer

import android.Manifest
import android.content.Intent
import android.content.pm.PackageManager
import android.net.Uri
import android.os.Build
import android.provider.Settings
import io.flutter.embedding.android.FlutterActivity
import io.flutter.embedding.engine.FlutterEngine
import io.flutter.plugin.common.MethodCall
import io.flutter.plugin.common.MethodChannel

/** Hosts Flutter and exposes the dimmer controls over a MethodChannel. */
class MainActivity : FlutterActivity() {
    private lateinit var store: ConfigStore

    override fun configureFlutterEngine(flutterEngine: FlutterEngine) {
        super.configureFlutterEngine(flutterEngine)
        store = ConfigStore(this)
        MethodChannel(flutterEngine.dartExecutor.binaryMessenger, CHANNEL)
            .setMethodCallHandler { call, result -> handle(call, result) }
    }

    private fun handle(call: MethodCall, result: MethodChannel.Result) {
        when (call.method) {
            "getState" -> result.success(snapshot())

            "setConfig" -> {
                val args = call.arguments as? Map<*, *>
                if (args == null) {
                    result.error("BAD_ARGS", "Expected a map", null)
                } else {
                    store.config = DimmerConfig.fromMap(args)
                    DimmerService.instance?.refresh()
                    result.success(null)
                }
            }

            "start" -> {
                if (!Settings.canDrawOverlays(this)) {
                    result.error("NO_PERMISSION", "Display over other apps is not allowed", null)
                } else {
                    DimmerService.start(this)
                    result.success(null)
                }
            }

            "stop" -> {
                DimmerService.stop(this)
                result.success(null)
            }

            "requestOverlayPermission" -> {
                startActivity(
                    Intent(
                        Settings.ACTION_MANAGE_OVERLAY_PERMISSION,
                        Uri.parse("package:$packageName"),
                    ),
                )
                result.success(null)
            }

            "requestNotificationPermission" -> {
                if (!notificationsAllowed()) {
                    requestPermissions(arrayOf(Manifest.permission.POST_NOTIFICATIONS), REQ_NOTIF)
                }
                result.success(null)
            }

            "openAppSettings" -> {
                startActivity(
                    Intent(
                        Settings.ACTION_APPLICATION_DETAILS_SETTINGS,
                        Uri.parse("package:$packageName"),
                    ),
                )
                result.success(null)
            }

            else -> result.notImplemented()
        }
    }

    private fun snapshot(): Map<String, Any> = store.config.toMap() + mapOf(
        "running" to DimmerService.running,
        "overlayPermission" to Settings.canDrawOverlays(this),
        "notificationPermission" to notificationsAllowed(),
    )

    private fun notificationsAllowed(): Boolean =
        Build.VERSION.SDK_INT < Build.VERSION_CODES.TIRAMISU ||
            checkSelfPermission(Manifest.permission.POST_NOTIFICATIONS) ==
            PackageManager.PERMISSION_GRANTED

    private companion object {
        const val CHANNEL = "dev.rex.screen_dimmer/dimmer"
        const val REQ_NOTIF = 1001
    }
}
