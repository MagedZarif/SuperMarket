using System.ComponentModel;
using System.Security.Claims;
using Azure.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperMarket.DBContext;
using SuperMarket.DTO;
using SuperMarket.models;
//review this page
namespace SuperMarket.Controllers
{
    
    [Route("superMarket/[controller]")]
    [ApiController]
    [Authorize]
    public class IItemController : Controller
    {

        private readonly APPDBContext _context;
        private readonly UserManager<IdentityUser> _userManager;


        public IItemController(APPDBContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        /// Get all IItems

        [HttpGet]
        public async Task<ActionResult<IEnumerable<IItem>>> GetIItems()
        {

            var items = new List<IItem>();
            items = await _context.Iitems.Include(i=>i.User).ToListAsync();

            return Ok(new
            {
                
                iitems = items.Select(i => new 
                {
                    ItemId = i.ItemId,
                    StartDate = i.StartDate,
                    ExpiredDate = i.ExpiredDate,
                    Price = i.Price,
                    qrcode = i.Qrcode,
                    IsSell = i.IsSell,
                    SaleId = i.SaleId,
                    username = i.User.UserName,
                }).OrderBy(i=>i.Price)
            });
        }

        //make one iitem get by firstordufault by price and itemid and not sell

        


        /// Get IItem by Id

        [HttpGet("{id}")]
        public async Task<ActionResult<IItemDTO>> GetIItem(int id)
        {
            var iitem = await _context.Iitems.FindAsync(id);
            if (iitem == null)
            {
                return NotFound(new { message = "IItem not found." });
            }
            
            var items = await _context.Iitems
                .Where(i => i.Id == id)
                .Include(i=>i.User)
                .Select(i => new 
                {
                    StartDate = i.StartDate,
                    ExpiredDate = i.ExpiredDate,
                    Price = i.Price,
                    IsSell = i.IsSell,
                    userId = i.User.Id,
                    userName = i.User.UserName,
                })
                .ToListAsync();

                return Ok(items);
        }


        [HttpGet("isExpired")]
        public async Task<ActionResult<IEnumerable<IItem>>> GetExpiredIItems()
        {
            var iitems = await _context.Iitems
                .Where(i => i.ExpiredDate < DateTime.UtcNow)
                .Include(i => i.Item).Include(i=>i.User)
                .ToListAsync();
            return Ok(new
            {
                iitems = iitems.Select(i => new
                {
                    ItemId = i.ItemId,
                    StartDate = i.StartDate,
                    ExpiredDate = i.ExpiredDate,
                    Price = i.Price,
                    Qrcode = i.Qrcode,
                    IsSell = i.IsSell,
                    SaleId = i.SaleId,
                    userId = i.userId,
                    username=i.User.UserName

                })
            });
        
    }

        [HttpGet("specificitem/{itemid}")]
        public async Task<ActionResult<IEnumerable<IItem>>> GetIItemsByItemId(int itemid)
        {
            var isItemExist = await _context.items.AnyAsync(i => i.Id == itemid);
            if (isItemExist == false)
                return NotFound(new { message = "Item not found." });

            var iitems = await _context.Iitems
                .Where(i => i.ItemId == itemid).Include(i=>i.User)
                .ToListAsync();
            
            
            return Ok(new
            {
                iitems = iitems.Select(i => new 
                {
                    ItemId = i.ItemId,
                    StartDate = i.StartDate,
                    ExpiredDate = i.ExpiredDate,
                    Price = i.Price,
                    Qrcode = i.Qrcode,
                    IsSell = i.IsSell,
                    SaleId = i.SaleId,
                    userId = i.userId,
                    username=i.User.UserName
                })
            });
        }


        /// Create a new IItem

        [HttpPost]
        public async Task<ActionResult<IItemDTO>> CreateIItem(IItemDTO iitemDto)
        {
            
            var userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userName))
            {
            
                return Unauthorized("User not found in token.");
            }
            
            var user = await _userManager.FindByNameAsync(userName);
            
            if (iitemDto.ExpiredDate <= iitemDto.StartDate)
            {
                return BadRequest(new { message = "Expired date must be after the start date." });
            }


            var iitem = new IItem
            {
                StartDate = iitemDto.StartDate ?? DateTime.MinValue,
                ExpiredDate = iitemDto.ExpiredDate ?? DateTime.MaxValue,
                Price = iitemDto.Price ?? 0.0,
                Qrcode = iitemDto.qrcode ?? null,
                IsSell =iitemDto.IsSell ?? false,
                ItemId = iitemDto.ItemId,
                userId = user.Id
            };

            _context.Iitems.Add(iitem);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                iitem.ItemId,
                iitem.StartDate,
                iitem.ExpiredDate,
                iitem.Price,
                iitem.Qrcode,
                iitem.IsSell,
                iitem.userId,
            });
        }

        //create any number of IItem
        [HttpPost("{numberOfIitems}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IItemDTO>> CreateXIItems(IItemDTO iitemDto,long numberOfIitems)
        { 
            var userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userName))
            {
            
                return Unauthorized("User not found in token.");
            }
            
            var user = await _userManager.FindByNameAsync(userName);
            if (iitemDto.ExpiredDate <= iitemDto.StartDate)
            {
                return BadRequest(new { message = "Expired date must be after the start date." });
            }

            if (numberOfIitems < 0)
            {
                return BadRequest(new { message = "Number of items must be greater than zero." });
            }

            var IitemList = new List<IItem>();
            for (int i = 0; i < numberOfIitems; i++)
            {
                var iitem = new IItem
                {
                    StartDate = iitemDto.StartDate ?? DateTime.MinValue,
                    ExpiredDate =iitemDto.ExpiredDate ?? DateTime.MaxValue,
                    Price =iitemDto.Price ?? 0.0,
                    Qrcode = iitemDto.qrcode ?? null,
                    IsSell = iitemDto.IsSell ?? false,
                    ItemId = iitemDto.ItemId,
                    userId = user.Id ?? String.Empty,
                };
                IitemList.Add(iitem);

            }
            
           await _context.Iitems.AddRangeAsync(IitemList);
            await _context.SaveChangesAsync();

            var insertedItem = IitemList.FirstOrDefault();

            return Ok(new
            {
                numberOfIitems,
                item = insertedItem != null ? new
                {
                    insertedItem.StartDate,
                    insertedItem.ExpiredDate,
                    insertedItem.Price,
                    insertedItem.IsSell,
                    insertedItem.ItemId,
                    insertedItem.Qrcode
                } : null
            });
        }


        /// Update an existing IItem

        [HttpPut("{id}")]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> UpdateIItem(int id, IItemDTO iitemDto)
        {
            var iitem = await _context.Iitems.FindAsync(id);
            if (iitem == null)
            {
                return NotFound(new { message = "IItem not found." });
            }

     
            if (iitemDto.ExpiredDate <= iitemDto.StartDate)
            {
                return BadRequest(new { message = "Expired date must be after the start date." });
            }

                             
            iitem.StartDate = iitemDto.StartDate??iitem.StartDate;
            iitem.ExpiredDate =iitemDto.ExpiredDate ?? iitem.ExpiredDate;
            iitem.Price = iitemDto.Price ?? iitem.Price;
            iitem.IsSell = iitemDto.IsSell ?? iitem.IsSell;
            iitem.Qrcode = iitemDto.qrcode ?? iitem.Qrcode;
            iitem.ItemId = iitemDto.ItemId;
            iitem.SaleId = iitemDto.SaleId ?? iitem.SaleId;
                                                                                                                   
            _context.Iitems.Update(iitem);
            await _context.SaveChangesAsync();

            return Ok(iitem);
        }

        [HttpPut("specificItem/{id}")]                                                                                                                                                  
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> UpdateIItemsByItemId(int id, IItemDTO iitemDto)
        {
            var isItemExist = await _context.items.AnyAsync(i => i.Id == id);
            if (isItemExist == false)
                return NotFound(new { message = "Item not found." });

            var iitems = await _context.Iitems
                .Where(i => i.ItemId == id)
                .ToListAsync();

            if (iitems.Count == 0)
                return NotFound(new { message = "IItem not found." });


            foreach (var iitem in iitems)
            {
                if(iitemDto.StartDate!=null)
                    iitem.StartDate = (DateTime)iitemDto.StartDate;
                if (iitemDto.ExpiredDate != null)
                    iitem.ExpiredDate = (DateTime)iitemDto.ExpiredDate;
                if (iitemDto.Price != null)
                    iitem.Price = (double)iitemDto.Price;
                if (iitemDto.IsSell != null)
                    iitem.IsSell = (bool)iitemDto.IsSell;
                if (iitemDto.qrcode != null)
                    iitem.Qrcode = (string)iitemDto.qrcode;

                iitem.ItemId = iitemDto.ItemId;
            }
            _context.Iitems.UpdateRange(iitems);

            await _context.SaveChangesAsync();
            return Ok(new { message=$"number of update items {iitems.Count}"});
        }

        /// Delete an IItem by Id

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteIItem(int id)
        {
            var iitem = await _context.Iitems.FindAsync(id);
            if (iitem == null)
            {
                return NotFound(new { message = "IItem not found." });
            }

            _context.Iitems.Remove(iitem);
            await _context.SaveChangesAsync();

            return Ok(iitem);
        }

        [HttpDelete("specificItem/{id}")]
        [Authorize (Roles = "Admin")]
        public async Task<IActionResult> DeleteIItemsByItemId(int id)
        {
            var isItemExist = await _context.items.AnyAsync(i => i.Id == id);
            if (isItemExist == false)
                return NotFound(new { message = "Item not found." });


            var iitems = await _context.Iitems
                .Where(i => i.ItemId == id)
                .ToListAsync();
            _context.Iitems.RemoveRange(iitems);
            await _context.SaveChangesAsync();
            return Ok(iitems);
        }






    }



    
}

