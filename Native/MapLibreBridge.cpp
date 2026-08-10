#include "MapLibreBridge.h"
#include <string>

// MapLibre Native integration point. The MapLibre C++ objects belong here;
// this C ABI keeps WPF interop independent from MapLibre's C++ ABI.
struct MapBridgeHandle {
    std::string mbtiles;
    std::string style;
    double latitude = 18.1096;
    double longitude = -77.2975;
    double zoom = 8.0;
};

void* mapbridge_create() { return new MapBridgeHandle(); }
void mapbridge_destroy(void* handle) { delete static_cast<MapBridgeHandle*>(handle); }

int mapbridge_load_mbtiles(void* handle, const char* mbtiles_path, const char* style_path) {
    if (!handle || !mbtiles_path || !style_path) return 0;
    auto* map = static_cast<MapBridgeHandle*>(handle);
    map->mbtiles = mbtiles_path;
    map->style = style_path;
    return 1;
}

void mapbridge_set_camera(void* handle, double latitude, double longitude, double zoom) {
    if (!handle) return;
    auto* map = static_cast<MapBridgeHandle*>(handle);
    map->latitude = latitude;
    map->longitude = longitude;
    map->zoom = zoom;
}
