using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tubes_POS_API.Controllers;
using Tubes_POS_API.Data;
using Tubes_POS_API.Entities;
using Tubes_POS_API.Models;
using Tubes_POS_API.Models.DTOs;
using Tubes_POS_API.Repositories;
using Tubes_POS_API.Services;

namespace Tubes_POS_API.Tests;

public class MenuControllerTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly MenuController _controller;

    public MenuControllerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();

        var service = new MenuService(new MenuRepository(_db));
        _controller = new MenuController(service);
    }

    // Tests that the menu list response is wrapped.
    [Fact]
    public void GetAll_ShouldReturnWrappedResponse()
    {
        var result = _controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<List<MenuResponse>>>(ok.Value);

        Assert.True(response.Success);
        Assert.NotEmpty(response.Data!);
    }

    // Tests that missing menu returns wrapped 404.
    [Fact]
    public void GetById_WhenMissing_ShouldReturnNotFoundWrapper()
    {
        var result = _controller.GetById(999);

        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<object>>(notFound.Value);

        Assert.False(response.Success);
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
