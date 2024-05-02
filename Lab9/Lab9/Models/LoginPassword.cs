namespace Lab9.Models
{
    public class LoginPassword
    {
        public int Id { get; set; }
        public string Login {  get; set; }
        public string Password { get; set; }
        public virtual List<User> Users { get; set; }
        public LoginPassword()
        {
            Users = new List<User>();   
        }
    }
}
