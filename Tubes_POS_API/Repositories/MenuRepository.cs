using Microsoft.EntityFrameworkCore;
using Tubes_POS_API.Data;
using Tubes_POS_API.Entities;

namespace Tubes_POS_API.Repositories;

public sealed class MenuRepository : IMenuRepository
{
    private readonly AppDbContext _db;

    public MenuRepository(AppDbContext db)
    {
        _db = db;
    }

    public List<Menu> GetAll()
    {
        return _db.Menus
            .AsNoTracking()
            .OrderBy(m => m.Id)
            .ToList();
    }

    public Menu? GetById(int id)
    {
        return _db.Menus.FirstOrDefault(m => m.Id == id);
    }

    public void Add(Menu menu)
    {
        _db.Menus.Add(menu);
        _db.SaveChanges();
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

        _db.SaveChanges();
    }

    public void Delete(int id)
    {
        var menu = GetById(id);

        if (menu != null)
        {
            _db.Menus.Remove(menu);
            _db.SaveChanges();
        }
    }
}
