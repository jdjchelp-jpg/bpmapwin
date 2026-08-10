#include "MapLibreBridge.h"
#include <algorithm>
#include <string>
#include <sqlite3.h>

// MapLibre Native integration point. The MapLibre C++ objects belong here;
// this C ABI keeps WPF interop independent from MapLibre's C++ ABI.
struct MapBridgeHandle {
    std::string mbtiles;
    std::string style;
    double latitude = 18.1096;
    double longitude = -77.2975;
    double zoom = 8.0;
    void* parent = nullptr;
};

void* mapbridge_create(void* parent_window) { auto* map = new MapBridgeHandle(); map->parent = parent_window; return map; }
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

int mapbridge_read_tile(void* handle, int zoom, int x, int y, unsigned char* buffer, int capacity) {
    if (!handle || !buffer || capacity <= 0) return -1;
    auto* map = static_cast<MapBridgeHandle*>(handle);
    sqlite3* db = nullptr;
    if (sqlite3_open_v2(map->mbtiles.c_str(), &db, SQLITE_OPEN_READONLY, nullptr) != SQLITE_OK) return -1;
    const int tms_y = (1 << zoom) - 1 - y;
    sqlite3_stmt* statement = nullptr;
    const char* sql = "SELECT tile_data FROM tiles WHERE zoom_level=? AND tile_column=? AND tile_row=?";
    int result = -1;
    if (sqlite3_prepare_v2(db, sql, -1, &statement, nullptr) == SQLITE_OK) {
        sqlite3_bind_int(statement, 1, zoom); sqlite3_bind_int(statement, 2, x); sqlite3_bind_int(statement, 3, tms_y);
        if (sqlite3_step(statement) == SQLITE_ROW) {
            const auto* data = static_cast<const unsigned char*>(sqlite3_column_blob(statement, 0));
            const int size = sqlite3_column_bytes(statement, 0);
            if (size <= capacity) { std::copy(data, data + size, buffer); result = size; }
        }
    }
    sqlite3_finalize(statement); sqlite3_close(db); return result;
}
