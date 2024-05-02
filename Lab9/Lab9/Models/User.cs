namespace Lab9.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int LoginPasswordId {  get; set; }
        public LoginPassword LoginPassword { get; set; }
        public virtual List<Seller> Sellers { get; set; }
        public virtual List<Customer> Customers { get; set; }
        public virtual List<Administrator> Administrators { get; set; }
        public User()
        {
            Sellers = new List<Seller>();
            Customers = new List<Customer>();
            Administrators = new List<Administrator>();
        }
    }
}
