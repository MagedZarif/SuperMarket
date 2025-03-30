namespace SuperMarket.DTO
{
    public class IItemDTO
    {

        public DateTime? StartDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public double? Price { get; set; }


        public bool? IsSell { get; set; } 
        public int ItemId { get; set; }
    }
}
