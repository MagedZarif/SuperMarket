using Microsoft.AspNetCore.Identity;

namespace SuperMarket.models
{
    public class IItem
    {

        public int Id { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime ExpiredDate { get; set; }
        public double Price { get; set; }
        public bool IsSell { get; set; } = false;

        public string? Qrcode { get; set; } = null!;
        
        public string userId { get; set; }
        
        public IdentityUser User { get; set; }
        

        public int ItemId { get; set; }
        public Item Item { get; set; } = null!;

        public int? SaleId { get; set; }
        public Sale Sale { get; set; } 

        internal static object Where(Func<object, bool> value)
        {
            throw new NotImplementedException();
        }
    }
}
