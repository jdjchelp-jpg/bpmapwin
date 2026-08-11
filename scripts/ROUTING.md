# Offline routing graph

The routing build uses OSRM's offline `extract`, `partition`, and `customize` stages. It creates a road graph from the same `.osm.pbf` source as the map tiles and search database.

For a local Docker-enabled machine:

```bash
bash scripts/build-routing.sh jamaica Maps/jamaica.osm.pbf Maps/jamaica
```

The resulting `*.osrm*` files must be shipped together. The graph is currently built for driving with OSRM's `car.lua` profile. Walking and cycling need their respective profiles and separate graph artifacts.
