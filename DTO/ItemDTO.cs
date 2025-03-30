namespace SuperMarket.DTO
{
    public class ItemDTO
    {
        public string Name { get; set; }

        public double Price { get; set; }
        public int Quantity { get; set; }

        //public DateTime StartDate { get; set; }
        //public DateTime ExpiryDate { get; set; }

        public int CategoryId { get; set; }

    }
}
