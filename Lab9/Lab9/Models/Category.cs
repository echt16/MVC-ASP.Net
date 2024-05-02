namespace Lab9.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual List<Kind> Kinds { get; set; }
        public Category()
        {
            Kinds = new List<Kind>();
        }
    }
}
