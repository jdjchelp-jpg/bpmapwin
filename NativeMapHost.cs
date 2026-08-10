using System.Runtime.InteropServices;
using System.Windows.Interop;
using OfflineMaps.Win.Core;

namespace OfflineMaps.Win;

public sealed class NativeMapHost : HwndHost
{
    private IntPtr _native;
    public OfflinePackage? Package { get; set; }

    protected override HandleRef BuildWindowCore(HandleRef hwndParent)
    {
        if (Package?.TilesPath is null) throw new InvalidOperationException("No offline MBTiles package was found.");
        _native = NativeMapBridge.Create(hwndParent.Handle);
        NativeMapBridge.LoadMbtiles(_native, Package.TilesPath, Package.StylePath);
        return new HandleRef(this, _native);
    }

    protected override void DestroyWindowCore(HandleRef hwnd)
    {
        if (_native != IntPtr.Zero) NativeMapBridge.Destroy(_native);
        _native = IntPtr.Zero;
    }
}

internal static partial class NativeMapBridge
{
    [LibraryImport("MapLibreBridge", EntryPoint = "mapbridge_create")]
    internal static partial IntPtr Create(IntPtr parent);
    [LibraryImport("MapLibreBridge", EntryPoint = "mapbridge_destroy")]
    internal static partial void Destroy(IntPtr handle);
    [LibraryImport("MapLibreBridge", EntryPoint = "mapbridge_load_mbtiles", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int LoadMbtiles(IntPtr handle, string mbtiles, string style);
}
