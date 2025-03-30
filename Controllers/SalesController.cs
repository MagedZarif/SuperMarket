using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SuperMarket.DBContext;
using System.Linq;
using System.Threading.Tasks;
using SuperMarket.models;

[Authorize] // Only logged-in users can access sales
public class SalesController : Controller
{
    private readonly APPDBContext _context;

    public SalesController(APPDBContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _context.categories.ToListAsync();
        return Json(categories);
    }

    [HttpGet]
    public async Task<IActionResult> GetItems(int categoryId)
    {
        var items = await _context.items.Where(i => i.CategoryId == categoryId).ToListAsync();
        return Json(items);
    }

    [HttpGet]
    public async Task<IActionResult> GetIItems(int itemId)
    {
        var iitems = await _context.Iitems
            .Where(i => i.ItemId == itemId && !i.IsSell)
            .OrderBy(i => i.StartDate) // Order by oldest start date
            .ToListAsync();
        return Json(iitems);
    }

    [HttpPost]
    public async Task<IActionResult> CompleteSale([FromBody] Sale sale)
    {
        _context.Add(sale);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Sale completed successfully!" });
    }
}
