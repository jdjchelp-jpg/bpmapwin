# MapLibre Native bridge

This folder isolates the C++ MapLibre Native integration from WPF. Build the DLL with CMake, then host the MapLibre native view in a WPF `HwndHost`/Windows child window and call the exported functions from C# P/Invoke. `NativeMapHost.cs` now performs that WPF lifecycle hookup when `MapLibreBridge.dll` is present.

The generated MBTiles package uses the OpenMapTiles layer names shown in `Maps/jamaica-style.json`. Replace its placeholder absolute path at install time with the discovered package path; do not commit machine-specific paths.

The C++ bridge now opens MBTiles read-only and exposes TMS-correct `z/x/y` tile reads. The next native build step is linking MapLibre Native and replacing the state-only window methods with the selected Windows renderer backend.

## CI build

Run the **Build native MapLibre bridge** workflow from GitHub Actions. It uses the official MapLibre Native Windows/MSVC path, then builds this bridge and uploads native artifacts. The current bridge build verifies the Windows toolchain and MBTiles reader; the final renderer link requires selecting the Windows backend target exposed by the MapLibre revision being used.
