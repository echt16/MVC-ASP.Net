namespace Lab9.Models
{
    public class CartViewModal
    {
        public List<bool> IsConsired { get; set; }
        public List<AddedToCartProduct> AddedToCartProducts {  get; set; }
        public List<Product> Products { get; set; } 
        public List<User> Sellers { get; set; }
        public double Sum {  get; set; }
        public CartViewModal()
        {
            AddedToCartProducts = new List<AddedToCartProduct>();
            Products = new List<Product>();
            Sellers = new List<User>();
            IsConsired = new List<bool>();  
        }
    }
}
