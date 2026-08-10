# MapLibre Native bridge

This folder isolates the C++ MapLibre Native integration from WPF. Build the DLL with CMake, then host the MapLibre native view in a WPF `HwndHost`/Windows child window and call the exported functions from C# P/Invoke.

The generated MBTiles package uses the OpenMapTiles layer names shown in `Maps/jamaica-style.json`. Replace its placeholder absolute path at install time with the discovered package path; do not commit machine-specific paths.

The current C++ file is a safe ABI scaffold. The next native build step is linking MapLibre Native and replacing the state-only methods with the selected Windows renderer backend.
