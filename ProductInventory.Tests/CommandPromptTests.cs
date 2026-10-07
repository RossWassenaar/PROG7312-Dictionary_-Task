namespace ProductInventory.Tests;

public class CommandPromptTests
{
    private readonly Inventory _inventory = Inventory.CreateWithSampleStock();
    private readonly CommandPrompt _prompt;

    public CommandPromptTests()
    {
        _prompt = new CommandPrompt(_inventory);
    }

    [Fact]
    public void Add_takes_a_multi_word_name()
    {
        string line = Assert.Single(_prompt.Execute("add 113 Wifi Router 1299.99"));

        Assert.StartsWith("ADDED #113 WIFI ROUTER", line);
        Assert.Equal("Wifi Router", _inventory.Find(113)!.Name);
        Assert.Equal(1299.99m, _inventory.Find(113)!.Price);
    }

    [Fact]
    public void Add_with_a_taken_id_is_refused()
    {
        Assert.Equal("ID #101 IS ALREADY TAKEN", Assert.Single(_prompt.Execute("ADD 101 Toaster 300")));
        Assert.Equal("Laptop", _inventory.Find(101)!.Name);
    }

    [Fact]
    public void Update_changes_the_product()
    {
        _prompt.Execute("update 101 Laptop Pro 6500");

        Assert.Equal("Laptop Pro", _inventory.Find(101)!.Name);
        Assert.Equal(6500m, _inventory.Find(101)!.Price);
    }

    [Fact]
    public void Find_works_by_id_and_by_name()
    {
        Assert.Contains("JBL SPEAKER", Assert.Single(_prompt.Execute("find 102")));
        Assert.Contains("TABLET", Assert.Single(_prompt.Execute("find tab")));
        Assert.Equal("NO PRODUCT WITH ID #999", Assert.Single(_prompt.Execute("find 999")));
    }

    [Fact]
    public void Delete_removes_the_product()
    {
        Assert.Equal("DELETED #104 WIRELESS MOUSE", Assert.Single(_prompt.Execute("delete 104")));
        Assert.Null(_inventory.Find(104));
    }

    [Fact]
    public void List_prints_every_product_then_a_count()
    {
        var output = _prompt.Execute("list");

        Assert.Equal(_inventory.Count + 1, output.Count);
        Assert.Equal($"{_inventory.Count} ITEMS", output[^1]);
    }

    [Theory]
    [InlineData("add 120 Kettle")]
    [InlineData("add abc Kettle 300")]
    [InlineData("add 120 Kettle free")]
    [InlineData("delete")]
    [InlineData("dance")]
    public void Bad_input_explains_itself_without_touching_the_stock(string input)
    {
        int before = _inventory.Count;

        Assert.NotEmpty(_prompt.Execute(input));
        Assert.Equal(before, _inventory.Count);
    }
}
