namespace Lab9.Models
{
    public class ProductDetailsViewModel
    {
        public Product Product { get; set; }
        public List<Photo> Photos { get; set; }

        public string Kind { get; set; }
        public string Category { get; set; }    
        public User Seller {  get; set; }
    }
}
