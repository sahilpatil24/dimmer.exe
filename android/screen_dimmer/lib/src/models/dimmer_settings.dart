import 'package:flutter/foundation.dart';

/// Immutable description of how the screen should be dimmed.
///
/// All values are plain percentages so they can cross the platform channel
/// untouched; the native side owns the actual color math.
@immutable
class DimmerSettings {
  const DimmerSettings({
    this.dim = defaultDim,
    this.filterEnabled = false,
    this.tint = defaultTint,
    this.panelLimitEnabled = false,
    this.panelLimit = defaultPanelLimit,
    this.resumeOnBoot = false,
  });

  static const int minDim = 0;
  static const int maxDim = 100;
  static const int minTint = -100; // cool / blue
  static const int maxTint = 100; // warm / yellow
  static const int minPanelLimit = 0;
  static const int maxPanelLimit = 100;

  static const int defaultDim = 40;
  static const int defaultTint = 30;
  static const int defaultPanelLimit = 10;

  /// Dim strength, 0..100. 100 is the darkest the app allows, never full black.
  final int dim;

  /// Whether the warm/cool color filter is applied.
  final bool filterEnabled;

  /// -100 (cool/blue) .. 0 (neutral) .. 100 (warm/yellow).
  final int tint;

  /// Whether the panel's own brightness is capped while the dimmer runs.
  final bool panelLimitEnabled;

  /// Panel brightness cap, 0 (lowest the panel supports) .. 100.
  final int panelLimit;

  /// Restart the dimmer after the phone reboots.
  final bool resumeOnBoot;

  DimmerSettings copyWith({
    int? dim,
    bool? filterEnabled,
    int? tint,
    bool? panelLimitEnabled,
    int? panelLimit,
    bool? resumeOnBoot,
  }) {
    return DimmerSettings(
      dim: _clamp(dim ?? this.dim, minDim, maxDim),
      filterEnabled: filterEnabled ?? this.filterEnabled,
      tint: _clamp(tint ?? this.tint, minTint, maxTint),
      panelLimitEnabled: panelLimitEnabled ?? this.panelLimitEnabled,
      panelLimit:
          _clamp(panelLimit ?? this.panelLimit, minPanelLimit, maxPanelLimit),
      resumeOnBoot: resumeOnBoot ?? this.resumeOnBoot,
    );
  }

  Map<String, Object> toMap() => <String, Object>{
        'dim': dim,
        'filterEnabled': filterEnabled,
        'tint': tint,
        'panelLimitEnabled': panelLimitEnabled,
        'panelLimit': panelLimit,
        'resumeOnBoot': resumeOnBoot,
      };

  factory DimmerSettings.fromMap(Map<Object?, Object?> map) {
    int intOf(String key, int fallback) =>
        (map[key] as num?)?.toInt() ?? fallback;
    bool boolOf(String key, bool fallback) => (map[key] as bool?) ?? fallback;

    return const DimmerSettings().copyWith(
      dim: intOf('dim', defaultDim),
      filterEnabled: boolOf('filterEnabled', false),
      tint: intOf('tint', defaultTint),
      panelLimitEnabled: boolOf('panelLimitEnabled', false),
      panelLimit: intOf('panelLimit', defaultPanelLimit),
      resumeOnBoot: boolOf('resumeOnBoot', false),
    );
  }

  @override
  bool operator ==(Object other) =>
      other is DimmerSettings &&
      other.dim == dim &&
      other.filterEnabled == filterEnabled &&
      other.tint == tint &&
      other.panelLimitEnabled == panelLimitEnabled &&
      other.panelLimit == panelLimit &&
      other.resumeOnBoot == resumeOnBoot;

  @override
  int get hashCode => Object.hash(
        dim,
        filterEnabled,
        tint,
        panelLimitEnabled,
        panelLimit,
        resumeOnBoot,
      );

  @override
  String toString() => 'DimmerSettings(${toMap()})';
}

int _clamp(int value, int lo, int hi) =>
    value < lo ? lo : (value > hi ? hi : value);
