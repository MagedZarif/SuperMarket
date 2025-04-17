using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace SuperMarket.models
{
    public class Sale
    {
        public int id { get; set; }
        public DateTime date { get; set; }= DateTime.Now;

        public double? total { get; set; }
        
       
        [Required]
        public string userId { get; set; }
        
        public IdentityUser User { get; set; }

        public ICollection<IItem> Iitems { get; set; } = new List<IItem>();



    }
}
