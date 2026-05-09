using Tubes_POS_API.Entities;
using Tubes_POS_API.Repositories;
using Tubes_POS_API.Services;

namespace Tubes_POS_API.Tests;

public class MenuServiceTests
{
    private readonly MenuService _service;

    public MenuServiceTests()
    {
        _service = new MenuService(new MenuRepository());
    }

    [Fact]
    public void GetAll_ShouldReturnSeededMenus()
    {
        var result = _service.GetAll();

        Assert.NotEmpty(result);
        Assert.Contains(result, menu => menu.Name == "Nasi Goreng");
        Assert.Contains(result, menu => menu.Name == "Es Teh");
    }

    [Fact]
    public void Add_ShouldAppendNewMenu()
    {
        var menu = new Menu
        {
            Name = "Kopi Susu",
            Description = "Minuman kopi susu",
            Price = 12_000m,
            Category = "Minuman"
        };

        _service.Add(menu);

        var result = _service.GetAll();

        Assert.Contains(result, item => item.Name == "Kopi Susu");
    }

    [Fact]
    public void Add_InvalidMenu_ShouldThrow()
    {
        var menu = new Menu
        {
            Name = string.Empty,
            Price = 0m,
            Category = string.Empty
        };

        var ex = Assert.Throws<Exception>(() => _service.Add(menu));

        Assert.Contains("Menu name cannot be empty", ex.Message);
    }

    [Fact]
    public void Update_ShouldModifyExistingMenu()
    {
        var menu = _service.GetById(1)!;
        menu.Name = "Nasi Goreng Spesial";
        menu.Price = 27_500m;

        _service.Update(menu);

        var updated = _service.GetById(1);

        Assert.NotNull(updated);
        Assert.Equal("Nasi Goreng Spesial", updated!.Name);
        Assert.Equal(27_500m, updated.Price);
    }

    [Fact]
    public void Delete_ShouldRemoveMenu()
    {
        var menu = new Menu
        {
            Id = 99,
            Name = "Test Menu",
            Price = 10_000m,
            Category = "Lainnya"
        };

        _service.Add(menu);
        Assert.NotNull(_service.GetById(99));

        _service.Delete(99);

        Assert.Null(_service.GetById(99));
    }
}
