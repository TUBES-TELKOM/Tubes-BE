using Tubes_POS_API.Entities;
using Tubes_POS_API.Repositories;

namespace Tubes_POS_API.Services;

public class MenuService : IMenuService
{
    private readonly IMenuRepository _menuRepository;

    public MenuService(IMenuRepository menuRepository)
    {
        _menuRepository = menuRepository;
    }

    public List<Menu> GetAll()
    {
        return _menuRepository.GetAll();
    }

    public Menu? GetById(int id)
    {
        return _menuRepository.GetById(id);
    }

    public void Add(Menu menu)
    {
        // DbC Validation
        if (string.IsNullOrWhiteSpace(menu.Name))
            throw new Exception("Menu name cannot be empty");

        if (menu.Price <= 0)
            throw new Exception("Price must be greater than 0");

        if (string.IsNullOrWhiteSpace(menu.Category))
            throw new Exception("Category cannot be empty");

        _menuRepository.Add(menu);
    }

    public void Update(Menu menu)
    {
        var existingMenu = _menuRepository.GetById(menu.Id);

        if (existingMenu == null)
            throw new Exception("Menu not found");

        if (string.IsNullOrWhiteSpace(menu.Name))
            throw new Exception("Menu name cannot be empty");

        if (menu.Price <= 0)
            throw new Exception("Price must be greater than 0");

        _menuRepository.Update(menu);
    }

    public void Delete(int id)
    {
        var existingMenu = _menuRepository.GetById(id);

        if (existingMenu == null)
            throw new Exception("Menu not found");

        _menuRepository.Delete(id);
    }
}