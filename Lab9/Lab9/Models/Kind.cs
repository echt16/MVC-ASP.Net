namespace Lab9.Models
{
    public class Kind
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public virtual List<Product> Products { get; set; }
        public Kind()
        {
            Products = new List<Product>();
        }
    }
}
