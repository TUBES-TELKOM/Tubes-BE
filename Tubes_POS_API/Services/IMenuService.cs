using Tubes_POS_API.Entities;

namespace Tubes_POS_API.Services;

public interface IMenuService
{
    List<Menu> GetAll();

    Menu? GetById(int id);

    void Add(Menu menu);

    void Update(Menu menu);

    void Delete(int id);
}