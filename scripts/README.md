# Region conversion pipeline

Install these tools and put them on `PATH`:

- `osmium-tool` — filters/export OSM PBF
- `tilemaker` — converts OSM PBF to vector-tile MBTiles
- Python 3.11+ — builds the SQLite FTS5 search index

Run from the repository root:

```powershell
.\scripts\build-region.ps1 -RegionId jamaica -InputPbf .\Maps\jamaica.osm.pbf
```

Outputs are written to `Maps/jamaica/`:

- `jamaica.mbtiles` — MapLibre vector tiles
- `jamaica-search.sqlite` — local FTS5 names/POIs database
- `manifest.json` — package inventory used by the app

The routing graph is intentionally a separate build artifact. Use Itinero for an embedded .NET graph or OSRM/Valhalla for a native service/library. The app should not claim a region is navigation-ready until `routing` in the manifest points to a verified graph.
