using System.IO;
using System.Windows;
using OfflineMaps.Win.Core;

namespace OfflineMaps.Win;

public partial class MainWindow : Window
{
    private readonly OfflineMapControl _map;
    private readonly string _mapsDirectory = Path.Combine(AppContext.BaseDirectory, "Maps");
    private readonly OfflineSearchService? _search;

    public MainWindow()
    {
        InitializeComponent();
        Directory.CreateDirectory(_mapsDirectory);
        var package = OfflinePackageLocator.Find(AppContext.BaseDirectory, "jamaica");
        _map = new OfflineMapControl(package.Directory, package.TilesPath);
        _map.Initialize();
        MapStatus.Text = _map.Status;
        var bridgePath = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "MapLibreBridge.dll"),
            Path.Combine(AppContext.BaseDirectory, "Native", "MapLibreBridge.dll")
        }.FirstOrDefault(File.Exists);
        if (package.TilesPath is not null && bridgePath is not null)
        {
            try
            {
                MapSurface.Child = new NativeMapHost { Package = package };
            }
            catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException or BadImageFormatException)
            {
                MapStatus.Text = "Offline package found, but the native map bridge could not load. Check MapLibreBridge.dll and its dependencies.";
            }
        }
        if (package.SearchDatabasePath is not null) _search = new OfflineSearchService(package.SearchDatabasePath);
    }

    private async void SearchBox_OnTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (_search is null) return;
        SearchResults.ItemsSource = await _search.SearchAsync(SearchBox.Text);
    }
    private void DownloadRegion_OnClick(object sender, RoutedEventArgs e) => MessageBox.Show("Region download is the next module. Add an MBTiles package to the Maps folder for this starter.", "Offline Maps");
    private void RecordVoice_OnClick(object sender, RoutedEventArgs e) => MessageBox.Show("Voice recording will create a reusable prompt pack. Record each prompt in a quiet room, preview it, then validate before enabling it for navigation.", "Voice guidance");
    private void ZoomIn_OnClick(object sender, RoutedEventArgs e) => _map.Zoom(1);
    private void ZoomOut_OnClick(object sender, RoutedEventArgs e) => _map.Zoom(-1);
}

public sealed class OfflineMapControl
{
    private readonly string _mapDirectory;
    private readonly string? _tilesPath;
    public string Status { get; private set; } = "Not initialized";
    public double ZoomLevel { get; private set; } = 10;
    public OfflineMapControl(string mapDirectory, string? tilesPath = null) { _mapDirectory = mapDirectory; _tilesPath = tilesPath; }
    public void Initialize()
    {
        var packages = Directory.EnumerateFiles(_mapDirectory, "*.mbtiles", SearchOption.AllDirectories).ToArray();
        Status = packages.Length == 0 ? "Place an MBTiles package in the Maps folder to render it." : $"Found {packages.Length} offline map package(s).";
        // Production adapter: bind Mapsui.MapControl here and add an MBTiles vector-tile layer.
    }
    public void Zoom(int delta) => ZoomLevel = Math.Clamp(ZoomLevel + delta, 1, 20);
}
