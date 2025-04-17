using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace SuperMarket.models
{
    [Index(nameof(name),IsUnique = true)]
    public class Category
    {
        public int id { get; set; }
        
        [Required]
        [StringLength(30, ErrorMessage = "Name cannot be longer than 30 characters.")]
        public String name { get; set; }
        public string? description { get; set; }

        List<Item> item { get; set; } = new List<Item>();

    }
}
