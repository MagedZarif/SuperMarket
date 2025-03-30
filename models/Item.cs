using System.ComponentModel;
using Microsoft.AspNetCore.Identity;

namespace SuperMarket.models
{
    public class Item
    {
        public int Id { get; set; }


        public string Name { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }


        public bool IsExpired { get; set; } = false;
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public ICollection<IItem> Iitems { get; set; } = new List<IItem>();

        //public string OwnerId { get; set; }
        //public IdentityUser Owner { get; set; }
    }
}
