using System.IO;
using System.Windows;

namespace OfflineMaps.Win;

public partial class MainWindow : Window
{
    private readonly OfflineMapControl _map;
    private readonly string _mapsDirectory = Path.Combine(AppContext.BaseDirectory, "Maps");

    public MainWindow()
    {
        InitializeComponent();
        Directory.CreateDirectory(_mapsDirectory);
        _map = new OfflineMapControl(_mapsDirectory);
        _map.Initialize();
        MapStatus.Text = _map.Status;
    }

    private void SearchBox_OnTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) { }
    private void DownloadRegion_OnClick(object sender, RoutedEventArgs e) => MessageBox.Show("Region download is the next module. Add an MBTiles package to the Maps folder for this starter.", "Offline Maps");
    private void RecordVoice_OnClick(object sender, RoutedEventArgs e) => MessageBox.Show("Voice recording will create a reusable prompt pack. Record each prompt in a quiet room, preview it, then validate before enabling it for navigation.", "Voice guidance");
    private void ZoomIn_OnClick(object sender, RoutedEventArgs e) => _map.Zoom(1);
    private void ZoomOut_OnClick(object sender, RoutedEventArgs e) => _map.Zoom(-1);
}

public sealed class OfflineMapControl
{
    private readonly string _mapDirectory;
    public string Status { get; private set; } = "Not initialized";
    public double ZoomLevel { get; private set; } = 10;
    public OfflineMapControl(string mapDirectory) => _mapDirectory = mapDirectory;
    public void Initialize()
    {
        var packages = Directory.EnumerateFiles(_mapDirectory, "*.mbtiles").ToArray();
        Status = packages.Length == 0 ? "Place an MBTiles package in the Maps folder to render it." : $"Found {packages.Length} offline map package(s).";
        // Production adapter: bind Mapsui.MapControl here and add an MBTiles vector-tile layer.
    }
    public void Zoom(int delta) => ZoomLevel = Math.Clamp(ZoomLevel + delta, 1, 20);
}
