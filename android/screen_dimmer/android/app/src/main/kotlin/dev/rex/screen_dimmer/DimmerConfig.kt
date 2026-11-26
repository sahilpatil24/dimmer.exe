package dev.rex.screen_dimmer

/** Settings shared between the Flutter UI, the service and the quick tile. */
data class DimmerConfig(
    val dimPercent: Int = 40,
    val filterEnabled: Boolean = false,
    /** -100 = cool/blue, 0 = neutral, 100 = warm/yellow. */
    val tintPercent: Int = 30,
    val panelLimitEnabled: Boolean = false,
    /** 0 = lowest panel brightness, 100 = full. */
    val panelLimitPercent: Int = 10,
    val resumeOnBoot: Boolean = false,
) {
    fun sanitized(): DimmerConfig = copy(
        dimPercent = dimPercent.coerceIn(0, 100),
        tintPercent = tintPercent.coerceIn(-100, 100),
        panelLimitPercent = panelLimitPercent.coerceIn(0, 100),
    )

    fun toMap(): Map<String, Any> = mapOf(
        "dim" to dimPercent,
        "filterEnabled" to filterEnabled,
        "tint" to tintPercent,
        "panelLimitEnabled" to panelLimitEnabled,
        "panelLimit" to panelLimitPercent,
        "resumeOnBoot" to resumeOnBoot,
    )

    companion object {
        fun fromMap(map: Map<*, *>): DimmerConfig {
            val d = DimmerConfig()
            return DimmerConfig(
                dimPercent = (map["dim"] as? Number)?.toInt() ?: d.dimPercent,
                filterEnabled = map["filterEnabled"] as? Boolean ?: d.filterEnabled,
                tintPercent = (map["tint"] as? Number)?.toInt() ?: d.tintPercent,
                panelLimitEnabled = map["panelLimitEnabled"] as? Boolean ?: d.panelLimitEnabled,
                panelLimitPercent = (map["panelLimit"] as? Number)?.toInt() ?: d.panelLimitPercent,
                resumeOnBoot = map["resumeOnBoot"] as? Boolean ?: d.resumeOnBoot,
            ).sanitized()
        }
    }
}
