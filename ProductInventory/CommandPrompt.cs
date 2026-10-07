namespace ProductInventory;

public class CommandPrompt
{
    public static readonly IReadOnlyList<string> HelpLines = new[]
    {
        "COMMANDS:",
        "  LIST",
        "  FIND <ID or NAME>",
        "  ADD <ID> <NAME> <PRICE>",
        "  UPDATE <ID> <NAME> <PRICE>",
        "  DELETE <ID>",
        "  TOTAL",
        "E.G. ADD 113 WIFI ROUTER 1299.99"
    };

    private readonly Inventory _inventory;

    public CommandPrompt(Inventory inventory)
    {
        _inventory = inventory;
    }

    public IReadOnlyList<string> Execute(string input)
    {
        string[] words = input.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
            return Array.Empty<string>();

        return words[0].ToLowerInvariant() switch
        {
            "help" or "?" => HelpLines,
            "list" or "display" => List(),
            "find" or "search" => Find(words),
            "add" => Save(words, isNew: true),
            "update" => Save(words, isNew: false),
            "delete" or "remove" => Delete(words),
            "total" => Total(),
            _ => new[] { $"UNKNOWN COMMAND \"{words[0].ToUpperInvariant()}\" - TRY HELP" }
        };
    }

    private IReadOnlyList<string> List()
    {
        var lines = _inventory.GetAll().Select(entry => Receipt.Row(entry.Key, entry.Value)).ToList();
        lines.Add($"{_inventory.Count} ITEMS");
        return lines;
    }

    private IReadOnlyList<string> Find(string[] words)
    {
        if (words.Length < 2)
            return new[] { "USAGE: FIND <ID or NAME>" };

        string query = string.Join(' ', words[1..]);

        if (int.TryParse(query, out int id))
        {
            Product? product = _inventory.Find(id);
            return new[] { product is null ? Receipt.Explain(InventoryResult.NotFound, id) : Receipt.Row(id, product) };
        }

        var matches = _inventory.SearchByName(query);
        if (matches.Count == 0)
            return new[] { $"NOTHING MATCHES \"{query.ToUpperInvariant()}\"" };

        return matches.Select(entry => Receipt.Row(entry.Key, entry.Value)).ToList();
    }

    private IReadOnlyList<string> Save(string[] words, bool isNew)
    {
        string verb = isNew ? "ADD" : "UPDATE";
        if (words.Length < 4)
            return new[] { $"USAGE: {verb} <ID> <NAME> <PRICE>" };
        if (!int.TryParse(words[1], out int id))
            return new[] { Receipt.Explain(InventoryResult.InvalidId, 0) };
        if (!RandFormat.TryParse(words[^1], out decimal price))
            return new[] { $"\"{words[^1]}\" ISN'T A PRICE" };

        string name = string.Join(' ', words[2..^1]);
        InventoryResult result = isNew ? _inventory.Add(id, name, price) : _inventory.Update(id, name, price);

        return result == InventoryResult.Success
            ? new[] { $"{(isNew ? "ADDED" : "UPDATED")} {Receipt.Row(id, _inventory.Find(id)!)}" }
            : new[] { Receipt.Explain(result, id) };
    }

    private IReadOnlyList<string> Delete(string[] words)
    {
        if (words.Length != 2 || !int.TryParse(words[1], out int id))
            return new[] { "USAGE: DELETE <ID>" };

        Product? product = _inventory.Find(id);
        if (product is null || !_inventory.Delete(id))
            return new[] { Receipt.Explain(InventoryResult.NotFound, id) };

        return new[] { $"DELETED #{id} {product.Name.ToUpperInvariant()}" };
    }

    private IReadOnlyList<string> Total() => new[]
    {
        $"{_inventory.Count} ITEMS IN STOCK",
        $"STOCK VALUE {RandFormat.Format(_inventory.TotalValue)}"
    };
}
