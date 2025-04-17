using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperMarket.DBContext;
using SuperMarket.DTO;
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
            var userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userName))
            {
            
                return Unauthorized("User not found in token.");
            }
            
            var user = await _userManager.FindByNameAsync(userName);
            
            
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
                Iitems = iitems,
                userId = user.Id
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


        [HttpGet("GetAvailableIItem/{itemId}/{numberOfItems}")]
        public async Task<ActionResult<IItem>> GetAvailableIItem([FromRoute] int itemId,[FromRoute] int numberOfItems = 1)
        {
            var primaryItems = await _context.Iitems
                .Where(i => i.ItemId == itemId && !i.IsSell && i.Qrcode == null)
                .Include(i => i.User)
                .OrderBy(i => i.StartDate)
                .Take(numberOfItems)
                .ToListAsync();

            
            var totalFetched = primaryItems;
            if (primaryItems.Count < numberOfItems)
            {
                int remaining = numberOfItems - primaryItems.Count;
                var secondaryItems = await _context.Iitems
                    .Where(i => i.ItemId == itemId && !i.IsSell && i.Qrcode != null)
                    .Include(i => i.User)
                    .OrderBy(i => i.StartDate)
                    .Take(remaining)
                    .ToListAsync();
                totalFetched.AddRange(secondaryItems);
            }


            if (totalFetched.Count < numberOfItems)
            {
                return BadRequest(new
                {
                    message = $"Only {totalFetched.Count} item(s) available out of {numberOfItems} requested."
                });
            }


            return Ok(totalFetched.Select(i=>new
            {
                i.Id,
                i.StartDate,
                i.ExpiredDate,
                i.Price,
                i.Qrcode,
                i.ItemId,
                i.IsSell,
                i.userId,
                i.User.UserName
            }));
        }
        
        
        [HttpGet("GetAvailableIItemByQrcode/{Qrcode}/{numberOfItems}")]
        public async Task<ActionResult<IItem>> GetAvailableIItemByQrcode([FromRoute] String Qrcode,[FromRoute]int numberOfItems=1)
        {
            
            var availableItems = await _context.Iitems
                .Where(i => i.Qrcode == Qrcode && i.IsSell == false)
                .Include(i=>i.User)
                .OrderBy(i => i.StartDate)
                .ToListAsync();

            if (!availableItems.Any())
            {
                return NotFound(new { message = "No available IItem found for the given Qrcode." });
            }

            if (availableItems.Count < numberOfItems)
            {
                return BadRequest(new { 
                    message = $"Only {availableItems.Count} item(s) available out of {numberOfItems} requested."
                });
            }

            var iitems = availableItems.Take(numberOfItems).ToList();
            return Ok(iitems.Select(i => new 
            {
                i.Id,
                i.StartDate,
                i.ExpiredDate,
                i.Price,
                i.Qrcode,
                i.ItemId,
                i.IsSell,
                i.userId,
                i.User.UserName
                ,
            }));
        }
        
        
        
        [HttpPost("itemInformation/{itemId}")]
        public async Task<IActionResult> GetItemsInformation([FromRoute]int itemId)
        {
            var Sale = await _context.sales.Where(i => i.Iitems.Any(i=>i.Id==itemId)).ToListAsync();
        
            return Ok(Sale);
        }


        [HttpPut("{saleId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSale([FromBody] SaleDTO sale, [FromRoute] int saleId)
        {
            var existingSale = await _context.sales.FindAsync(saleId);
            if (existingSale == null)
                return NotFound();


            if (sale.total.HasValue)
                existingSale.total = sale.total;

            if (!string.IsNullOrEmpty(sale.userId))
                existingSale.userId = sale.userId;

            //date fromat
            //2023-12-25T00:00:00
            if (sale.date.HasValue)
            {

                existingSale.date = new DateTime(
                    sale.date.Value.Year != 1 ? sale.date.Value.Year : existingSale.date.Year,
                    sale.date.Value.Month != 1 ? sale.date.Value.Month : existingSale.date.Month,
                    sale.date.Value.Day != 1 ? sale.date.Value.Day : existingSale.date.Day,
                    sale.date.Value.Hour != 1 ? sale.date.Value.Hour : existingSale.date.Hour,
                    sale.date.Value.Minute != 1 ? sale.date.Value.Minute : existingSale.date.Minute,
                    sale.date.Value.Second != 1 ? sale.date.Value.Second : existingSale.date.Second,
                    0

                );
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Sale updated successfully!", Sale = existingSale });
        }

        [HttpDelete("{saleId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSale(int saleId)
        {
            var sale = await _context.sales.FindAsync(saleId);
            if (sale == null)
                return NotFound();

            _context.sales.Remove(sale);
            await _context.SaveChangesAsync();

            return Ok(new { message = "sale deleted successfully!", Sale = sale });
        }
    }
}

