namespace Lab9.Models
{
    public class AddedToCartProduct
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public int? ProductId { get; set; }
        public Product? Product { get; set; }
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }
    }
}
