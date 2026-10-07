namespace ProductInventory;

public static class Receipt
{
    public const int NameWidth = 15;

    public static string Row(int id, Product product)
    {
        string name = product.Name.ToUpperInvariant();
        if (name.Length > NameWidth)
            name = name[..(NameWidth - 1)] + "…";

        return $"#{id} {name.PadRight(NameWidth)} {RandFormat.Format(product.Price),11}";
    }

    public static string Explain(InventoryResult result, int id) => result switch
    {
        InventoryResult.Success => "OK",
        InventoryResult.DuplicateId => $"ID #{id} IS ALREADY TAKEN",
        InventoryResult.NotFound => $"NO PRODUCT WITH ID #{id}",
        InventoryResult.InvalidId => "ID MUST BE A WHOLE NUMBER ABOVE 0",
        InventoryResult.InvalidName => "NAME CAN'T BE BLANK",
        InventoryResult.InvalidPrice => "PRICE MUST BE MORE THAN R0",
        _ => result.ToString().ToUpperInvariant()
    };
}
