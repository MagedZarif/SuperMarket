using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperMarket.DBContext;
using SuperMarket.DTO;
using SuperMarket.models;

[Route("superMarket/[controller]")]
[ApiController]
[Authorize]
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
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CategoryDTO model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
            return BadRequest("Category name is required.");
        
        var category = new Category
        {
            name = model.Name,
            description = model.Description
        };
        _context.categories.Add(category);
        await _context.SaveChangesAsync();
        return Ok(model);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, CategoryDTO model)
    {
        var category = await _context.categories.FindAsync(id);
        if (category == null)
            return NotFound();

        category.name = model.Name?? category.name;
        category.description = model.Description?? category.description;
        await _context.SaveChangesAsync();

        return Ok(category);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
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
