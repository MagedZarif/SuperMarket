namespace SuperMarket.models
{
    public class Sale
    {
        public int id { get; set; }
        public DateTime date { get; set; }= DateTime.Now;

        public double? total { get; set; }

        public ICollection<IItem> Iitems { get; set; } = new List<IItem>();



    }
}
