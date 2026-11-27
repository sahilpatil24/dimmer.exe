import 'package:flutter/material.dart';

import 'dimmer_settings.dart';

/// A named combination of dim level, color filter and optional panel limit.
@immutable
class DimmerPreset {
  const DimmerPreset({
    required this.id,
    required this.label,
    required this.icon,
    required this.dim,
    required this.filterEnabled,
    required this.tint,
    this.panelLimit,
  });

  final String id;
  final String label;
  final IconData icon;
  final int dim;
  final bool filterEnabled;
  final int tint;

  /// null leaves the panel brightness alone.
  final int? panelLimit;

  /// Applies this preset while keeping unrelated settings (e.g. boot resume).
  DimmerSettings applyTo(DimmerSettings current) => current.copyWith(
        dim: dim,
        filterEnabled: filterEnabled,
        tint: tint,
        panelLimitEnabled: panelLimit != null,
        panelLimit: panelLimit,
      );

  bool matches(DimmerSettings s) =>
      s.dim == dim &&
      s.filterEnabled == filterEnabled &&
      (!filterEnabled || s.tint == tint) &&
      s.panelLimitEnabled == (panelLimit != null) &&
      (panelLimit == null || s.panelLimit == panelLimit);
}

abstract final class DimmerPresets {
  static const List<DimmerPreset> all = <DimmerPreset>[
    DimmerPreset(
      id: 'reading',
      label: 'Reading',
      icon: Icons.menu_book,
      dim: 15,
      filterEnabled: true,
      tint: 25,
    ),
    DimmerPreset(
      id: 'cinema',
      label: 'Cinema',
      icon: Icons.movie,
      dim: 45,
      filterEnabled: false,
      tint: 0,
    ),
    DimmerPreset(
      id: 'evening',
      label: 'Evening',
      icon: Icons.wb_twilight,
      dim: 30,
      filterEnabled: true,
      tint: 40,
    ),
    DimmerPreset(
      id: 'night',
      label: 'Night',
      icon: Icons.nightlight_round,
      dim: 55,
      filterEnabled: true,
      tint: 60,
    ),
    DimmerPreset(
      id: 'deep_night',
      label: 'Deep night',
      icon: Icons.bedtime,
      dim: 75,
      filterEnabled: true,
      tint: 75,
      panelLimit: 20,
    ),
    DimmerPreset(
      id: 'maximum',
      label: 'Maximum',
      icon: Icons.dark_mode,
      dim: 100,
      filterEnabled: true,
      tint: 80,
      panelLimit: 0,
    ),
  ];
}
