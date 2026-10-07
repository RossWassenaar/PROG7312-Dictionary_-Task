namespace ProductInventory.Tests;

public class InventoryTests
{
    [Fact]
    public void Sample_stock_has_at_least_ten_products()
    {
        var inventory = Inventory.CreateWithSampleStock();

        Assert.True(inventory.Count >= 10);
        Assert.Equal("Laptop", inventory.Find(101)!.Name);
        Assert.Equal(5000m, inventory.Find(101)!.Price);
    }

    [Fact]
    public void Add_stores_a_new_product_under_its_id()
    {
        var inventory = new Inventory();

        Assert.Equal(InventoryResult.Success, inventory.Add(200, "  Router ", 1299.99m));
        Product product = inventory.Find(200)!;
        Assert.Equal("Router", product.Name);
        Assert.Equal(1299.99m, product.Price);
    }

    [Fact]
    public void Add_refuses_an_id_that_is_already_taken()
    {
        var inventory = new Inventory();
        inventory.Add(200, "Router", 1299m);

        Assert.Equal(InventoryResult.DuplicateId, inventory.Add(200, "Modem", 999m));
        Assert.Equal("Router", inventory.Find(200)!.Name);
    }

    [Theory]
    [InlineData(0, "Router", 10, InventoryResult.InvalidId)]
    [InlineData(-5, "Router", 10, InventoryResult.InvalidId)]
    [InlineData(1, "   ", 10, InventoryResult.InvalidName)]
    [InlineData(1, "Router", 0, InventoryResult.InvalidPrice)]
    [InlineData(1, "Router", -1, InventoryResult.InvalidPrice)]
    public void Add_rejects_invalid_details(int id, string name, int price, InventoryResult expected)
    {
        var inventory = new Inventory();

        Assert.Equal(expected, inventory.Add(id, name, price));
        Assert.Equal(0, inventory.Count);
    }

    [Fact]
    public void Update_changes_name_and_price()
    {
        var inventory = Inventory.CreateWithSampleStock();

        Assert.Equal(InventoryResult.Success, inventory.Update(101, "Gaming Laptop", 15999.5m));
        Assert.Equal("Gaming Laptop", inventory.Find(101)!.Name);
        Assert.Equal(15999.50m, inventory.Find(101)!.Price);
    }

    [Fact]
    public void Update_of_a_missing_id_reports_not_found()
    {
        Assert.Equal(InventoryResult.NotFound, new Inventory().Update(999, "Ghost", 1m));
    }

    [Fact]
    public void Find_returns_null_for_a_missing_id()
    {
        Assert.Null(Inventory.CreateWithSampleStock().Find(999));
    }

    [Fact]
    public void SearchByName_is_case_insensitive_and_matches_part_of_a_name()
    {
        var match = Assert.Single(Inventory.CreateWithSampleStock().SearchByName("SPEAK"));

        Assert.Equal(102, match.Key);
    }

    [Fact]
    public void Delete_removes_the_product()
    {
        var inventory = Inventory.CreateWithSampleStock();
        int before = inventory.Count;

        Assert.True(inventory.Delete(103));
        Assert.Null(inventory.Find(103));
        Assert.Equal(before - 1, inventory.Count);
        Assert.False(inventory.Delete(103));
    }

    [Fact]
    public void GetAll_is_ordered_by_id()
    {
        var inventory = new Inventory();
        inventory.Add(30, "C", 3m);
        inventory.Add(10, "A", 1m);
        inventory.Add(20, "B", 2m);

        Assert.Equal(new[] { 10, 20, 30 }, inventory.GetAll().Select(entry => entry.Key));
    }

    [Fact]
    public void TotalValue_adds_up_every_price()
    {
        var inventory = new Inventory();
        inventory.Add(1, "A", 10.50m);
        inventory.Add(2, "B", 4.25m);

        Assert.Equal(14.75m, inventory.TotalValue);
    }
}
