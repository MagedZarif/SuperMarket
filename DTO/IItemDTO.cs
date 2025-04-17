using System.Runtime.CompilerServices;
using SuperMarket.models;

namespace SuperMarket.DTO
{
    public class IItemDTO
    {

        public DateTime? StartDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public double? Price { get; set; }
        public String? qrcode { get; set; }
        
        public bool? IsSell { get; set; } 
        public int ItemId { get; set; }
        public int? SaleId { get; set; }
        
    }
}
