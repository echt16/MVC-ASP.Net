using Microsoft.EntityFrameworkCore;
namespace Lab9.Models
{
    public class ShopDbContext : DbContext
    {
        public DbSet<AddedToCartProduct> AddedToCartProducts { get; set; }
        public DbSet<Administrator> Administrators { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Deal> Deals { get; set; }
        public DbSet<Kind> Kinds { get; set; }
        public DbSet<LoginPassword> LoginsPasswords { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Seller> Sellers { get; set; }
        public DbSet<User> Users { get; set; }
        public ShopDbContext(DbContextOptions<ShopDbContext> options) : base(options) { Database.EnsureCreated(); }
    }
}
