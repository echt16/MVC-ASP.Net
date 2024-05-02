namespace Lab9.Models
{
    public class Product
    {
        public int Id { get; set; } 
        public string Name { get; set; }
        public string Description { get; set; }
        public string LogoSource { get; set; }
        public virtual List<Photo> Photos { get; set; }
        public int KindId { get; set; }
        public Kind Kind { get; set; }
        public int SellerId { get; set; }
        public Seller Seller { get; set; }
        public double Price { get; set; }
        public virtual List<Deal> Deals { get; set; }
        public virtual List<AddedToCartProduct> AddedToCartProducts { get; set; }
        public Product()
        {
            Photos = new List<Photo>();
            Deals = new List<Deal>();
            AddedToCartProducts = new List<AddedToCartProduct>();
        }
    }
}
