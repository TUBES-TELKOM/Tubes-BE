using Tubes_POS_API.Entities;

namespace Tubes_POS_API.Repositories;

public class MenuRepository : IMenuRepository
{
    private static List<Menu> menus = new()
    {
        new Menu
        {
            Id = 1,
            Name = "Nasi Goreng",
            Description = "Nasi goreng spesial",
            Price = 20000,
            Category = "Makanan"
        },

        new Menu
        {
            Id = 2,
            Name = "Es Teh",
            Description = "Minuman dingin",
            Price = 5000,
            Category = "Minuman"
        }
    };

    public List<Menu> GetAll()
    {
        return menus;
    }

    public Menu? GetById(int id)
    {
        return menus.FirstOrDefault(x => x.Id == id);
    }

    public void Add(Menu menu)
    {
        menus.Add(menu);
    }

    public void Update(Menu menu)
    {
        var existingMenu = GetById(menu.Id);

        if (existingMenu == null)
            return;

        existingMenu.Name = menu.Name;
        existingMenu.Description = menu.Description;
        existingMenu.Price = menu.Price;
        existingMenu.Category = menu.Category;
        existingMenu.IsAvailable = menu.IsAvailable;
        existingMenu.UpdatedAt = DateTime.UtcNow;
    }

    public void Delete(int id)
    {
        var menu = GetById(id);

        if (menu != null)
        {
            menus.Remove(menu);
        }
    }
}