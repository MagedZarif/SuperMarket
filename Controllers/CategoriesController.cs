using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperMarket.DBContext;
using SuperMarket.models;

[Route("superMarket/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class CategoriesController : ControllerBase
{
    private readonly APPDBContext _context;

    public CategoriesController(APPDBContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _context.categories.ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create(Category model)
    {
        _context.categories.Add(model);
        await _context.SaveChangesAsync();
        return Ok(model);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Category model)
    {
        var category = await _context.categories.FindAsync(id);
        if (category == null)
            return NotFound();

        category.name = model.name;
        await _context.SaveChangesAsync();

        return Ok(category);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.categories.FindAsync(id);
        if (category == null)
            return NotFound();

        _context.categories.Remove(category);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
