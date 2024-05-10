namespace Lab9.Models
{
    public class DealsViewModal
    {
        public List<Deal> Deals { get; set; }
        public List<Product> Products { get; set; }
        public List<User> Sellers { get; set; }
        public double Sum { get; set; }
        public DealsViewModal()
        {
            Deals = new List<Deal>();
            Products = new List<Product>();
            Sellers = new List<User>();
        }
    }
}
