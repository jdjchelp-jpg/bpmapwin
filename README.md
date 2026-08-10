# Offline Maps for Windows

A native WPF/.NET 8 starter for an offline OSM map application. The UI shell and local map-package discovery are implemented without requiring network access.

## Engine direction

For a C++-first production engine, use **MapLibre Native** for rendering and a small C++/CLI or C ABI bridge to WPF. Keep the bridge limited to lifecycle, camera, tile-source, and gesture calls. Use **OsmAnd C++ Core** instead if bundled search, routing, and navigation are more important than a minimal rendering stack; it is heavier but closer to a complete offline navigation product. Do not mix both engines in the first milestone.

Recommended first milestone: MapLibre Native + MBTiles/vector tiles for rendering, a separate routing core, and SQLite/FTS5 for search.

## Offline downloads and data

The region catalog should return signed manifests, not arbitrary URLs. Each manifest contains region ID, display name, bounds, format, size, version, SHA-256, and required style metadata. Download to a `.partial` file, support HTTP range resume, verify SHA-256, atomically rename to the final package, and write an install record. Keep `.pbf` source extracts for building graphs/search indexes and `.mbtiles`/`.mvt` packages for fast map display; the renderer should not parse raw PBF on every startup.

## Offline search

Import each region into SQLite with FTS5. Use normalized `name`, `street`, `city`, and `category` columns plus latitude/longitude and a bounding-box index. Query suggestions locally with a debounce, rank exact prefix matches first, then street/city/POI matches, and never fall back to network geocoding.

## Voice guidance and custom recordings

`VoiceMode.SystemTextToSpeech` uses Windows speech synthesis for unlimited dynamic text. `VoiceMode.RecordedVoicePack` uses prerecorded clips for stable prompts such as `turn_left`, `turn_right`, `roundabout_exit_2`, `traffic_light`, `stop_sign`, `rerouting`, and distance units. Dynamic street names and distances can remain TTS, or the recorder can optionally capture number/unit clips. A voice pack should contain a manifest, WAV/PCM files, language metadata, version, and a validation report. The app must preview clips, permit re-recording, require all mandatory keys, and let users disable/delete packs. Avoid recording while driving; provide a hands-free test and volume preview before navigation.

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

The new `Core/OfflineContracts.cs` file defines the seams for the C++ map engine, region downloader, offline geocoder, and both TTS/recorded voice implementations. `Core/VoicePackManifest.cs` defines the portable voice-pack manifest format.

`Core/RegionDownloadService.cs` now provides resumable package download, SHA-256 verification, and atomic installation. Replace the placeholder URL and hash in `regions.example.json` with a catalog hosted by your own release pipeline; never ship a catalog with unverifiable packages.

### Try the Jamaica download now

1. Copy `regions.example.json` to `regions.json`.
2. Point the service `HttpClient.BaseAddress` at the folder containing `regions.json`, or temporarily load the JSON from disk.
3. Call `DownloadAsync` with the Jamaica entry. The file will be saved under `Maps/jamaica.osm.pbf`.

The Geofabrik Jamaica extract is a valid OSM PBF source and is currently listed on its regional download page ([Jamaica extract](https://download.geofabrik.de/central-america/jamaica.html)). An empty `sha256` is supported only for development. Before releasing the app, obtain the published checksum and require it.

Important: the downloaded PBF is source data; MapLibre still needs a generated MBTiles/PMTiles vector-tile package. The next conversion step should generate rendering tiles, the FTS5 search database, and the routing graph from this PBF.

## Build

The environment used to generate this starter has the .NET runtime but not the .NET SDK. On a Windows machine with the .NET 8 SDK installed:

```powershell
dotnet build .\OfflineMaps.Win.csproj
dotnet run --project .\OfflineMaps.Win.csproj
```

## Build with GitHub Actions

You do not need Osmium or Tilemaker installed locally. In GitHub, open **Actions**, select **Build Jamaica offline map**, choose **Run workflow**, select `jamaica` or `cayman-islands`, and download the generated region artifact when it finishes. Jamaica uses Geofabrik; Cayman Islands uses the current Cayman PBF extract published by GEO2day. Both are OSM-derived and subject to the ODbL attribution requirements. [Cayman extract](https://geo2day.com/central_america/cayman_islands.html)
