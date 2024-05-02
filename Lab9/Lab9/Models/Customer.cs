namespace Lab9.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public virtual List<Deal> Deals { get; set; }
        public virtual List<AddedToCartProduct> AddedToCartProducts { get; set; }
        public Customer()
        {
            Deals = new List<Deal>();
            AddedToCartProducts = new List<AddedToCartProduct>();
        }
    }
}
