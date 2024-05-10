namespace Lab9.Models
{
    public class ProductViewModel
    {
        public List<Product> Products { get; set; }
        public List<Kind> Kinds { get; set; }
        public List<Category> Categories { get; set; }
        public ProductViewModel()
        {
            Products = new List<Product>();
            Kinds = new List<Kind>();
            Categories = new List<Category>();
        }
    }
}
