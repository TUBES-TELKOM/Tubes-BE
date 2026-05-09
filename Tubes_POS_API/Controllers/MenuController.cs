using Microsoft.AspNetCore.Mvc;
using Tubes_POS_API.Entities;
using Tubes_POS_API.Services;

namespace Tubes_POS_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuController : ControllerBase
{
    private readonly IMenuService _menuService;

    public MenuController(IMenuService menuService)
    {
        _menuService = menuService;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_menuService.GetAll());
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var menu = _menuService.GetById(id);

        if (menu == null)
            return NotFound();

        return Ok(menu);
    }

    [HttpPost]
    public IActionResult Add(Menu menu)
    {
        _menuService.Add(menu);

        return Ok(menu);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, Menu menu)
    {
        menu.Id = id;

        _menuService.Update(menu);

        return Ok(menu);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        _menuService.Delete(id);

        return Ok("Menu deleted");
    }
}