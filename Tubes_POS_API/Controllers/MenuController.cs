using Microsoft.AspNetCore.Mvc;
using Tubes_POS_API.Entities;
using Tubes_POS_API.Models;
using Tubes_POS_API.Models.DTOs;
using Tubes_POS_API.Services;

namespace Tubes_POS_API.Controllers;

[ApiController]
[Route("api/menus")]
public class MenuController : ControllerBase
{
    private readonly IMenuService _menuService;

    public MenuController(IMenuService menuService)
    {
        _menuService = menuService;
    }

    [HttpGet]
    public ActionResult<ApiResponse<List<MenuResponse>>> GetAll()
    {
        var result = _menuService.GetAll();

        return Ok(new ApiResponse<List<MenuResponse>>
        {
            Message = $"Ditemukan {result.Count} menu.",
            Data = result.Select(MapToResponse).ToList()
        });
    }

    [HttpGet("{id}")]
    public ActionResult<ApiResponse<MenuResponse>> GetById(int id)
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

        return Ok(new ApiResponse<MenuResponse>
        {
            Message = "Detail menu.",
            Data = MapToResponse(menu)
        });
    }

    [HttpPost]
    public ActionResult<ApiResponse<MenuResponse>> Add([FromBody] MenuRequest request)
    {
        var menu = new Menu
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Category = request.Category,
            IsAvailable = request.IsAvailable
        };

        _menuService.Add(menu);

        return Ok(new ApiResponse<MenuResponse>
        {
            Message = "Menu berhasil ditambahkan.",
            Data = MapToResponse(menu)
        });
    }

    [HttpPut("{id}")]
    public ActionResult<ApiResponse<MenuResponse>> Update(int id, [FromBody] MenuRequest request)
    {
        var menu = new Menu
        {
            Id = id,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Category = request.Category,
            IsAvailable = request.IsAvailable
        };

        _menuService.Update(menu);

        return Ok(new ApiResponse<MenuResponse>
        {
            Message = "Menu berhasil diperbarui.",
            Data = MapToResponse(menu)
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

    private static MenuResponse MapToResponse(Menu menu)
    {
        return new MenuResponse
        {
            Id = menu.Id,
            Name = menu.Name,
            Description = menu.Description,
            Price = menu.Price,
            Category = menu.Category,
            IsAvailable = menu.IsAvailable,
            CreatedAt = menu.CreatedAt,
            UpdatedAt = menu.UpdatedAt
        };
    }
}
