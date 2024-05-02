namespace Lab9.Models
{
    public class Photo
    {
        public int Id { get; set; }
        public string Source {  get; set; }
        public int ProductId {  get; set; }
        public Product Product { get; set; }
    }
}
