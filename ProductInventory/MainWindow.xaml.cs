using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ProductInventory;

public record PriceTag(int Id, string IdText, string Name, string Rands, string Cents, double Angle,
    bool IsHit, bool IsDimmed, bool IsSelected);

public partial class MainWindow : Window
{
    private const string SearchHint = "ENTER AN ID (KEY LOOKUP) OR PART OF A NAME";
    private static readonly double[] Tilts = { -2.5, 1.5, -1, 2.5, -3, 1, 2, -1.5, 3 };

    private readonly Inventory _inventory = Inventory.CreateWithSampleStock();
    private readonly CommandPrompt _prompt;
    private readonly ObservableCollection<string> _till = new();
    private readonly DispatcherTimer _stickerTimer = new() { Interval = TimeSpan.FromSeconds(1.8) };
    private HashSet<int>? _hits;
    private int? _selectedId;

    public MainWindow()
    {
        InitializeComponent();

        _prompt = new CommandPrompt(_inventory);
        TillLines.ItemsSource = _till;
        TillDate.Text = DateTime.Now.ToString("yyyy-MM-dd   HH:mm", CultureInfo.InvariantCulture);
        SearchSummary.Text = SearchHint;
        _stickerTimer.Tick += (_, _) => HideSticker();

        DrawStarburst(ValueBurst, ValueBurstShadow, spikes: 22, outer: 98, inner: 80);
        DrawStarburst(CountBurst, CountBurstShadow, spikes: 16, outer: 85, inner: 66);
        Laser.BeginAnimation(OpacityProperty, new DoubleAnimation(1, 0.35, TimeSpan.FromSeconds(0.7))
        {
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever
        });

        Print("WELCOME! TYPE HELP FOR COMMANDS");
        RefreshWall();
        Loaded += (_, _) => SearchBox.Focus();
    }

    private void RefreshWall()
    {
        TagWall.ItemsSource = _inventory.GetAll()
            .Select((entry, i) =>
            {
                bool isHit = _hits?.Contains(entry.Key) == true;
                return new PriceTag(
                    entry.Key,
                    $"#{entry.Key}",
                    entry.Value.Name.ToUpperInvariant(),
                    RandFormat.Rands(entry.Value.Price),
                    RandFormat.Cents(entry.Value.Price),
                    Tilts[i % Tilts.Length],
                    isHit,
                    _hits is not null && !isHit,
                    entry.Key == _selectedId);
            })
            .ToList();

        CountText.Text = _inventory.Count.ToString(CultureInfo.InvariantCulture);
        ValueText.Text = RandFormat.FormatWhole(_inventory.TotalValue);
    }

    private void ScrollTagIntoView(int id)
    {
        var tags = (List<PriceTag>)TagWall.ItemsSource;
        int index = tags.FindIndex(tag => tag.Id == id);
        if (index < 0)
            return;

        TagWall.UpdateLayout();
        if (TagWall.ItemContainerGenerator.ContainerFromIndex(index) is FrameworkElement container)
            container.BringIntoView();
    }

    private void Print(string line)
    {
        _till.Add(line);
        TillScroll.ScrollToEnd();
    }

    private void RunPriceCheck()
    {
        string query = SearchBox.Text.Trim();
        SweepLaser();

        if (query.Length == 0)
        {
            ShowEverything();
            return;
        }

        if (int.TryParse(query, out int id))
        {
            Product? product = _inventory.Find(id);
            _hits = product is null ? new HashSet<int>() : new HashSet<int> { id };

            if (product is null)
            {
                SearchSummary.Text = $"KEY LOOKUP #{id}: NOT IN STOCK";
                Fail("NOT FOUND!", $"CHECK #{id}: NOT FOUND");
            }
            else
            {
                SearchSummary.Text = $"KEY LOOKUP #{id}: {product.Name.ToUpperInvariant()} AT {RandFormat.Format(product.Price)}";
                Print($"CHECK {Receipt.Row(id, product)}");
            }
        }
        else
        {
            var matches = _inventory.SearchByName(query);
            _hits = matches.Select(match => match.Key).ToHashSet();

            string word = query.ToUpperInvariant();
            SearchSummary.Text = matches.Count switch
            {
                0 => $"NOTHING CALLED \"{word}\"",
                1 => $"1 MATCH FOR \"{word}\"",
                _ => $"{matches.Count} MATCHES FOR \"{word}\""
            };
            Print($"SEARCH \"{word}\": {matches.Count} FOUND");
            if (matches.Count == 0)
                ShowSticker("NO LUCK!", isGood: false);
        }

        RefreshWall();
        if (_hits.Count > 0)
            ScrollTagIntoView(_hits.Min());
    }

    private void ShowEverything()
    {
        _hits = null;
        SearchBox.Clear();
        SearchSummary.Text = SearchHint;
        RefreshWall();
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ReadCoupon(out int id, out string name, out decimal price))
            return;

        InventoryResult result = _inventory.Add(id, name, price);
        if (result != InventoryResult.Success)
        {
            Fail(result == InventoryResult.DuplicateId ? "ID TAKEN!" : "NOPE!", Receipt.Explain(result, id));
            return;
        }

        Print($"ADDED {Receipt.Row(id, _inventory.Find(id)!)}");
        ShowSticker("ADDED!", isGood: true);
        Select(id);
    }

    private void UpdateButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ReadCoupon(out int id, out string name, out decimal price))
            return;

        InventoryResult result = _inventory.Update(id, name, price);
        if (result != InventoryResult.Success)
        {
            Fail(result == InventoryResult.NotFound ? "NOT FOUND!" : "NOPE!", Receipt.Explain(result, id));
            return;
        }

        Print($"UPDATED {Receipt.Row(id, _inventory.Find(id)!)}");
        ShowSticker("UPDATED!", isGood: true);
        Select(id);
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ReadId(out int id))
            return;

        Product? product = _inventory.Find(id);
        if (product is null || !_inventory.Delete(id))
        {
            Fail("NOT FOUND!", Receipt.Explain(InventoryResult.NotFound, id));
            return;
        }

        Print($"DELETED #{id} {product.Name.ToUpperInvariant()}");
        ShowSticker("GONE!", isGood: true);
        ClearCoupon();
    }

    private bool ReadId(out int id)
    {
        if (int.TryParse(IdBox.Text.Trim(), out id) && id > 0)
            return true;
        return Fail("BAD ID!", Receipt.Explain(InventoryResult.InvalidId, 0));
    }

    private bool ReadCoupon(out int id, out string name, out decimal price)
    {
        name = NameBox.Text.Trim();
        price = 0;

        if (!ReadId(out id))
            return false;
        if (name.Length == 0)
            return Fail("NO NAME!", Receipt.Explain(InventoryResult.InvalidName, id));
        if (!RandFormat.TryParse(PriceBox.Text, out price) || price <= 0)
            return Fail("BAD PRICE!", Receipt.Explain(InventoryResult.InvalidPrice, id));
        return true;
    }

    private void Select(int id)
    {
        Product? product = _inventory.Find(id);
        if (product is null)
            return;

        _selectedId = id;
        IdBox.Text = id.ToString(CultureInfo.InvariantCulture);
        NameBox.Text = product.Name;
        PriceBox.Text = product.Price.ToString("0.00", CultureInfo.InvariantCulture);
        RefreshWall();
        ScrollTagIntoView(id);
    }

    private void ClearCoupon()
    {
        _selectedId = null;
        IdBox.Clear();
        NameBox.Clear();
        PriceBox.Clear();
        RefreshWall();
    }

    private void RunPrompt()
    {
        string command = PromptBox.Text.Trim();
        if (command.Length == 0)
            return;

        Print($"> {command.ToUpperInvariant()}");
        foreach (string line in _prompt.Execute(command))
            Print(line);

        PromptBox.Clear();
        RefreshWall();
    }

    private bool Fail(string sticker, string line)
    {
        ShowSticker(sticker, isGood: false);
        Print(line);
        return false;
    }

    private void ShowSticker(string text, bool isGood)
    {
        StickerText.Text = text;
        Sticker.Background = (Brush)FindResource(isGood ? "SaleRed" : "Ink");
        StickerText.Foreground = isGood ? Brushes.White : (Brush)FindResource("FlyerYellow");
        Sticker.Visibility = Visibility.Visible;

        var pop = new DoubleAnimation(0.2, 1, TimeSpan.FromMilliseconds(260))
        {
            EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.6 }
        };
        StickerScale.BeginAnimation(ScaleTransform.ScaleXProperty, pop);
        StickerScale.BeginAnimation(ScaleTransform.ScaleYProperty, pop);

        _stickerTimer.Stop();
        _stickerTimer.Start();
    }

    private void HideSticker()
    {
        _stickerTimer.Stop();
        Sticker.Visibility = Visibility.Collapsed;
    }

    private void SweepLaser() =>
        LaserShift.BeginAnimation(TranslateTransform.YProperty,
            new DoubleAnimation(0, Math.Max(0, SearchBox.ActualHeight - 8), TimeSpan.FromMilliseconds(220))
            {
                AutoReverse = true
            });

    private static void DrawStarburst(Path burst, Path shadow, int spikes, double outer, double inner)
    {
        var figure = new PathFigure { IsClosed = true };
        for (int i = 0; i < spikes * 2; i++)
        {
            double radius = i % 2 == 0 ? outer : inner;
            double angle = Math.PI * i / spikes;
            var point = new Point(outer + radius * Math.Sin(angle), outer - radius * Math.Cos(angle));

            if (i == 0)
                figure.StartPoint = point;
            else
                figure.Segments.Add(new LineSegment(point, isStroked: true));
        }

        var geometry = new PathGeometry(new[] { figure });
        geometry.Freeze();
        burst.Data = geometry;
        shadow.Data = geometry;
    }

    private void ScanButton_Click(object sender, RoutedEventArgs e) => RunPriceCheck();

    private void ShowAllButton_Click(object sender, RoutedEventArgs e) => ShowEverything();

    private void ClearButton_Click(object sender, RoutedEventArgs e) => ClearCoupon();

    private void EnterButton_Click(object sender, RoutedEventArgs e) => RunPrompt();

    private void SearchBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            RunPriceCheck();
    }

    private void PromptBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            RunPrompt();
    }

    private void Tag_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: PriceTag tag })
            Select(tag.Id);
    }
}
