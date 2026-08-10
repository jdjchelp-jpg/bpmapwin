# Offline Maps for Windows

A native WPF/.NET 8 starter for an offline OSM map application. The UI shell and local map-package discovery are implemented without requiring network access.

## Recommended architecture

- **UI:** WPF + MVVM, with `MainWindow` replaced by views/view-models as features grow.
- **Rendering:** Mapsui for .NET, backed by a local MBTiles/vector-tile provider. Keep rendering behind `IOfflineMapRenderer`.
- **Map packages:** Regional `.mbtiles` or `.pbf` bundles stored under `%LOCALAPPDATA%/OfflineMaps/Maps`, with a manifest containing region, version, bounds, and checksums.
- **Search:** SQLite + FTS5 tables for names, addresses, and POIs; import from OSM extracts and query asynchronously.
- **Routing:** Itinero is the simplest embedded .NET option for driving/walking/cycling. Keep a `IRouteEngine` boundary so Valhalla can replace it later.
- **Navigation:** A route instruction pipeline enriches maneuvers with OSM traffic signals, stop signs, lane data, and landmarks; `Windows.Media.SpeechSynthesis` speaks prompts.

Suggested database tables: `places(id, name, normalized_name, category, lat, lon)`, `addresses(place_id, house_number, street, city)`, `roads(id, geometry, profile_flags)`, and `map_metadata(region, version, checksum)`.

## Roadmap

1. **Shell and storage:** complete the current UI, map-package manifest, checksum validation, and download/resume manager.
2. **Rendering:** add Mapsui + MBTiles vector rendering, styles, gestures, GPS marker, and map rotation.
3. **Search:** import regional OSM data into SQLite/FTS5 and add ranked autocomplete with category filters.
4. **Routing:** build Itinero graphs per region and expose driving, walking, and cycling profiles.
5. **Navigation:** generate spoken maneuver queues, read traffic controls/lanes from OSM, show HUD, and reroute on deviation.
6. **Production hardening:** package signed regional data, migration/versioning, crash recovery, accessibility, and offline integration tests.

## Build

The environment used to generate this starter has the .NET runtime but not the .NET SDK. On a Windows machine with the .NET 8 SDK installed:

```powershell
dotnet build .\OfflineMaps.Win.csproj
dotnet run --project .\OfflineMaps.Win.csproj
```
