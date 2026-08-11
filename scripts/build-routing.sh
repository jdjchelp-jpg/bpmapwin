#!/usr/bin/env bash
set -euo pipefail

REGION_ID="${1:?region id required}"
PBF="${2:?input pbf required}"
ROOT="${3:-Maps/$REGION_ID}"
mkdir -p "$ROOT"
IMAGE="osrm/osrm-backend:latest"

docker run --rm -t -v "$(pwd):/data" "$IMAGE" osrm-extract -p /opt/car.lua "/data/$PBF"
GRAPH="${PBF%.osm.pbf}.osrm"
docker run --rm -t -v "$(pwd):/data" "$IMAGE" osrm-partition "/data/$GRAPH"
docker run --rm -t -v "$(pwd):/data" "$IMAGE" osrm-customize "/data/$GRAPH"

mv "$GRAPH"* "$ROOT/"
echo "Routing graph created in $ROOT"
