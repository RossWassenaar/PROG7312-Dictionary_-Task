namespace ProductInventory;

public enum InventoryResult
{
    Success,
    DuplicateId,
    NotFound,
    InvalidId,
    InvalidName,
    InvalidPrice
}

public class Inventory
{
    private readonly Dictionary<int, Product> _products = new();

    public int Count => _products.Count;

    public decimal TotalValue => _products.Values.Sum(product => product.Price);

    public InventoryResult Add(int id, string name, decimal price)
    {
        InventoryResult problem = Validate(id, name, price);
        if (problem != InventoryResult.Success)
            return problem;

        return _products.TryAdd(id, new Product(name.Trim(), RoundToCents(price)))
            ? InventoryResult.Success
            : InventoryResult.DuplicateId;
    }

    public InventoryResult Update(int id, string name, decimal price)
    {
        InventoryResult problem = Validate(id, name, price);
        if (problem != InventoryResult.Success)
            return problem;

        if (!_products.TryGetValue(id, out Product? product))
            return InventoryResult.NotFound;

        product.Name = name.Trim();
        product.Price = RoundToCents(price);
        return InventoryResult.Success;
    }

    public Product? Find(int id) => _products.TryGetValue(id, out Product? product) ? product : null;

    public IReadOnlyList<KeyValuePair<int, Product>> SearchByName(string text) =>
        _products
            .Where(entry => entry.Value.Name.Contains(text.Trim(), StringComparison.OrdinalIgnoreCase))
            .OrderBy(entry => entry.Key)
            .ToList();

    public bool Delete(int id) => _products.Remove(id);

    public IReadOnlyList<KeyValuePair<int, Product>> GetAll() => _products.OrderBy(entry => entry.Key).ToList();

    public static Inventory CreateWithSampleStock()
    {
        var inventory = new Inventory();
        inventory.Add(101, "Laptop", 5000m);
        inventory.Add(102, "JBL Speaker", 8000m);
        inventory.Add(103, "Tablet", 2300m);
        inventory.Add(104, "Wireless Mouse", 349.99m);
        inventory.Add(105, "Mechanical Keyboard", 1199m);
        inventory.Add(106, "Curved Monitor", 3999m);
        inventory.Add(107, "USB-C Hub", 499m);
        inventory.Add(108, "HD Webcam", 899.95m);
        inventory.Add(109, "Gaming Headset", 1499.50m);
        inventory.Add(110, "Smartwatch", 2999m);
        inventory.Add(111, "1TB External SSD", 1799m);
        inventory.Add(112, "Inkjet Printer", 2499m);
        return inventory;
    }

    private static InventoryResult Validate(int id, string name, decimal price)
    {
        if (id <= 0)
            return InventoryResult.InvalidId;
        if (string.IsNullOrWhiteSpace(name))
            return InventoryResult.InvalidName;
        if (price <= 0)
            return InventoryResult.InvalidPrice;
        return InventoryResult.Success;
    }

    private static decimal RoundToCents(decimal price) => Math.Round(price, 2, MidpointRounding.AwayFromZero);
}
