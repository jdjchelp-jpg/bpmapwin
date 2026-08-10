cmake_minimum_required(VERSION 3.25)
project(MapLibreBridge LANGUAGES CXX)
set(CMAKE_CXX_STANDARD 20)
find_package(SQLite3 REQUIRED)
add_library(MapLibreBridge SHARED MapLibreBridge.cpp MapLibreBridge.h)
target_compile_definitions(MapLibreBridge PRIVATE MAPBRIDGE_BUILD)
target_link_libraries(MapLibreBridge PRIVATE SQLite::SQLite3)
# Add MapLibre Native with your chosen Windows renderer/backend here.
# The bridge deliberately exposes only a C ABI to the WPF application.
