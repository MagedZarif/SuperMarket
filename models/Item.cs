using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace SuperMarket.models
{
     [Index(nameof(Name), IsUnique = true)]
    public class Item
    {
        public int Id { get; set; }


        [Required]
        [StringLength(30, ErrorMessage = "Name cannot be longer than 30 characters.")]
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
