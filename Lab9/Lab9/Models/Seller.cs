namespace Lab9.Models
{
    public class Seller
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public virtual List<Product> Products { get; set; }
    }
}
