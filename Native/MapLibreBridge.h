#pragma once

#ifdef _WIN32
#define MAPBRIDGE_API __declspec(dllexport)
#else
#define MAPBRIDGE_API
#endif

extern "C" {
    MAPBRIDGE_API void* mapbridge_create();
    MAPBRIDGE_API void mapbridge_destroy(void* handle);
    MAPBRIDGE_API int mapbridge_load_mbtiles(void* handle, const char* mbtiles_path, const char* style_path);
    MAPBRIDGE_API void mapbridge_set_camera(void* handle, double latitude, double longitude, double zoom);
}
