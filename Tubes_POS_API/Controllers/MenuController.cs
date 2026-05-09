using Microsoft.AspNetCore.Mvc;
using Tubes_POS_API.Entities;
using Tubes_POS_API.Models;
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
    public ActionResult<ApiResponse<List<Menu>>> GetAll()
    {
        var result = _menuService.GetAll();

        return Ok(new ApiResponse<List<Menu>>
        {
            Message = $"Ditemukan {result.Count} menu.",
            Data = result
        });
    }

    [HttpGet("{id}")]
    public ActionResult<ApiResponse<Menu>> GetById(int id)
    {
        var menu = _menuService.GetById(id);

        if (menu == null)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Menu tidak ditemukan.",
                Data = null
            });
        }

        return Ok(new ApiResponse<Menu>
        {
            Message = "Detail menu.",
            Data = menu
        });
    }

    [HttpPost]
    public ActionResult<ApiResponse<Menu>> Add([FromBody] Menu menu)
    {
        _menuService.Add(menu);

        return Ok(new ApiResponse<Menu>
        {
            Message = "Menu berhasil ditambahkan.",
            Data = menu
        });
    }

    [HttpPut("{id}")]
    public ActionResult<ApiResponse<Menu>> Update(int id, [FromBody] Menu menu)
    {
        menu.Id = id;

        _menuService.Update(menu);

        return Ok(new ApiResponse<Menu>
        {
            Message = "Menu berhasil diperbarui.",
            Data = menu
        });
    }

    [HttpDelete("{id}")]
    public ActionResult<ApiResponse<object>> Delete(int id)
    {
        _menuService.Delete(id);

        return Ok(new ApiResponse<object>
        {
            Message = "Menu berhasil dihapus.",
            Data = null
        });
    }
}
