using System.ComponentModel.DataAnnotations;


namespace SuperMarket.DTO
{
    public class SaleDTO
    {

        public DateTime? date { get; set; } = DateTime.Now;

        public double? total { get; set; }

        [Required] public String? userId { get; set; }


}
}
