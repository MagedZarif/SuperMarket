using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperMarket.DBContext;
using SuperMarket.models;

namespace SuperMarket.Controllers
{


    [Route("superMarket/[controller]")]
    [ApiController]
    [Authorize]
    public class SaleController : ControllerBase
    {
        private readonly APPDBContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public SaleController(APPDBContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var sales = await _context.sales
                .Include(s => s.Iitems)
                .ThenInclude(i => i.Item)
                .ToListAsync();
            return Ok(sales);
        }



        [HttpPost]
        public async Task<IActionResult> CreateSale([FromBody] List<int> iitemIds)
        {
            if (iitemIds == null || !iitemIds.Any())
                return BadRequest(new { message = "No items selected for sale." });

            // Check for already sold items
            var alreadySoldItems = await _context.Iitems
                .Where(i => iitemIds.Contains(i.Id) && i.IsSell)
                .Select(i => i.Id)
                .ToListAsync();

            if (alreadySoldItems.Any())
            {
                return BadRequest(new
                {
                    message = "this items are already sold.",
                    soldItemIds = alreadySoldItems
                });
            }
            var iitems = await _context.Iitems
                .Where(i => iitemIds.Contains(i.Id) && !i.IsSell)
                .ToListAsync();

            if (!iitems.Any())
                return NotFound(new { message = "No available items found." });

       
            double totalPrice = iitems.Sum(i => i.Price);



            var sale = new Sale
            {
                total = totalPrice,
                date = DateTime.UtcNow,
                Iitems = iitems
            };

            // Update the items as sold
            foreach (var iitem in iitems)
            {
                iitem.IsSell = true;
                iitem.Sale = sale;
            }

            _context.sales.Add(sale);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Sale completed!", saleId = sale.id, totalPrice });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Sale model)
        {
            var sale = await _context.sales.FindAsync(id);
            if (sale == null)
                return NotFound();

            sale.date = model.date;
            sale.total = model.total;
            await _context.SaveChangesAsync();
            return Ok(sale);
        }


        [HttpGet("GetAvailableIItem")]
        public async Task<ActionResult<IItem>> GetAvailableIItem([FromBody]int itemId)
        {
            var iitem = await _context.Iitems
                .Where(i => i.ItemId == itemId && i.IsSell == false)
                .OrderBy(i => i.StartDate) 
                .FirstOrDefaultAsync();

            if (iitem == null)
            {
                return NotFound(new { message = "No available IItem found for the given ItemId." });
            }

            return Ok(iitem);
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var sale = await _context.sales.FindAsync(id);
            if (sale == null)
                return NotFound();

            _context.sales.Remove(sale);
            await _context.SaveChangesAsync();
            return Ok(sale);
        }

    }
}
