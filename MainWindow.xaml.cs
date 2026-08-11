using System.IO;
using IOPath = System.IO.Path;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using OfflineMaps.Win.Core;

namespace OfflineMaps.Win;

public partial class MainWindow : Window
{
    private readonly OfflineMapControl _map;
    private readonly string _mapsDirectory = IOPath.Combine(AppContext.BaseDirectory, "Maps");
    private readonly OfflineSearchService? _search;
    private OfflineRoutePlanner? _routePlanner;

    public MainWindow()
    {
        InitializeComponent();
        Directory.CreateDirectory(_mapsDirectory);
        var package = OfflinePackageLocator.Find(AppContext.BaseDirectory, "jamaica");
        _map = new OfflineMapControl(package.Directory, package.TilesPath);
        _map.Initialize();
        MapStatus.Text = _map.Status;
        DrawMapBackground();
        var bridgePath = new[]
        {
            IOPath.Combine(AppContext.BaseDirectory, "MapLibreBridge.dll"),
            IOPath.Combine(AppContext.BaseDirectory, "Native", "MapLibreBridge.dll")
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
        if (package.SearchDatabasePath is not null)
        {
            _search = new OfflineSearchService(package.SearchDatabasePath);
            _routePlanner = new OfflineRoutePlanner(_search);
        }
    }

    private async void SearchBox_OnTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (_search is null) return;
        SearchResults.ItemsSource = await _search.SearchAsync(SearchBox.Text);
    }
    private async void CalculateRoute_OnClick(object sender, RoutedEventArgs e)
    {
        if (_routePlanner is null) { RouteStatus.Text = "Offline search database is not available."; return; }
        if (string.IsNullOrWhiteSpace(StartBox.Text) || string.IsNullOrWhiteSpace(DestinationBox.Text)) { RouteStatus.Text = "Enter both a start and destination."; return; }
        var stops = StopsBox.Text.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var plan = await _routePlanner.PlanAsync(StartBox.Text, stops, DestinationBox.Text);
        if (plan is null) { RouteStatus.Text = "Start or destination was not found offline."; return; }
        RouteStatus.Text = $"Preview: {plan.DistanceKm:F1} km · {plan.Request.Stops.Count} stop(s). {plan.Status}";
        DrawRoute(plan);
    }
    private void DownloadRegion_OnClick(object sender, RoutedEventArgs e) => MessageBox.Show("Region download is the next module. Add an MBTiles package to the Maps folder for this starter.", "Offline Maps");
    private void RecordVoice_OnClick(object sender, RoutedEventArgs e) => MessageBox.Show("Voice recording will create a reusable prompt pack. Record each prompt in a quiet room, preview it, then validate before enabling it for navigation.", "Voice guidance");
    private void ZoomIn_OnClick(object sender, RoutedEventArgs e) => _map.Zoom(1);
    private void ZoomOut_OnClick(object sender, RoutedEventArgs e) => _map.Zoom(-1);

    private void DrawMapBackground()
    {
        MapCanvas.Children.Clear();
        for (int i = 0; i < 18; i++) MapCanvas.Children.Add(new Line { X1 = i * 90, X2 = i * 90, Y1 = 0, Y2 = 1100, Stroke = new SolidColorBrush(Color.FromArgb(35, 80, 120, 140)) });
        for (int i = 0; i < 14; i++) MapCanvas.Children.Add(new Line { X1 = 0, X2 = 1700, Y1 = i * 80, Y2 = i * 80, Stroke = new SolidColorBrush(Color.FromArgb(35, 80, 120, 140)) });
    }

    private void DrawRoute(RoutePlan plan)
    {
        DrawMapBackground();
        var locations = new[] { plan.Request.Start }.Concat(plan.Request.Stops).Append(plan.Request.Destination).Select(x => x.Location).ToArray();
        var points = locations.Select(ToMapPoint).ToArray();
        MapCanvas.Children.Add(new Polyline { Points = new PointCollection(points), Stroke = new SolidColorBrush(Color.FromRgb(7, 122, 117)), StrokeThickness = 7, StrokeLineJoin = PenLineJoin.Round });
        for (int i = 0; i < points.Length; i++)
        {
            var color = i == 0 ? Color.FromRgb(20, 92, 160) : i == points.Length - 1 ? Color.FromRgb(210, 65, 67) : Color.FromRgb(239, 166, 45);
            var marker = new Ellipse { Width = 18, Height = 18, Fill = new SolidColorBrush(color), Stroke = Brushes.White, StrokeThickness = 3 };
            Canvas.SetLeft(marker, points[i].X - 9); Canvas.SetTop(marker, points[i].Y - 9); MapCanvas.Children.Add(marker);
        }
        MapEmptyState.Visibility = Visibility.Collapsed;
    }

    private static Point ToMapPoint(SearchResult location)
    {
        const double minLon = -78.6, maxLon = -76.0, minLat = 17.6, maxLat = 18.6, width = 1350, height = 820;
        return new Point(Math.Clamp((location.Longitude - minLon) / (maxLon - minLon) * width, 30, width - 30), Math.Clamp((maxLat - location.Latitude) / (maxLat - minLat) * height, 30, height - 30));
    }
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
