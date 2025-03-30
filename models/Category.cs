namespace SuperMarket.models
{
    public class Category
    {
        public int id { get; set; }
        public String name { get; set; }
        public string description { get; set; }

        List<Category> categories { get; set; } = new List<Category>();

    }
}
