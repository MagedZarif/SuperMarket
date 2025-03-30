using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperMarket.DBContext;
using SuperMarket.DTO;
using SuperMarket.models;

[Route("superMarket/[controller]")]
[ApiController]
[Authorize]
public class ItemsController : ControllerBase
{
    private readonly APPDBContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public ItemsController(APPDBContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {

        var items = await _context.items
       .Select(item => new
       {
        item.Id,
        item.Name,
        item.CategoryId,
        item.Price,
        Quantity = _context.Iitems.Count(i => i.ItemId == item.Id),
        IsExpired = _context.Iitems.Any(i => i.ItemId == item.Id && i.ExpiredDate < DateTime.UtcNow)
       })
       .ToListAsync();



        return Ok(await _context.items.Include(i => i.Category).ToListAsync());
    }


    [HttpGet("category/{categoryId}")]
    public async Task<IActionResult> GetItemsByCategory(int categoryId)
    {
        // Check if the category exists (optional, but good practice)
        var categoryExists = await _context.categories.AnyAsync(c => c.id == categoryId);
        if (!categoryExists)
            return NotFound($"Category with ID {categoryId} does not exist.");

        // Fetch items that belong to this category
        var items = await _context.items
            .Where(i => i.CategoryId == categoryId)
            .Include(i => i.Category) 
            .ToListAsync();

        return Ok(items);
    }


    [HttpPost]
    public async Task<IActionResult> AddItem(ItemDTO model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var category = await _context.categories.FindAsync(model.CategoryId);
        if (category == null)
            return NotFound("Category not found");

        var item = new Item
        {
            Name = model.Name,
            Price = model.Price,
            Quantity = model.Quantity,
    
            CategoryId = model.CategoryId
        };

        _context.items.Add(item);
        await _context.SaveChangesAsync();

        return Ok(item);
    }

    [HttpPut("{id}")]
    [Authorize(Roles ="Admin")]
    public async Task<IActionResult> Update(int id, Item updatedItem)
    {
        var item = await _context.items.FindAsync(id);
        if (item == null)
            return NotFound();

        var userId = _userManager.GetUserId(User);
        var isAdmin = User.IsInRole("Admin");

        //if (item.OwnerId != userId && !isAdmin)
        //    return Unauthorized();

        item.Name = updatedItem.Name;
        item.Price = updatedItem.Price;
        item.Quantity = updatedItem.Quantity;
        item.CategoryId = updatedItem.CategoryId;

        await _context.SaveChangesAsync();
        return Ok(item);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _context.items.FindAsync(id);
        if (item == null)
            return NotFound();

        _context.items.Remove(item);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
