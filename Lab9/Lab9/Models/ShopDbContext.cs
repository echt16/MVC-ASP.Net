
using Microsoft.EntityFrameworkCore;
namespace Lab9.Models
{
    public class ShopDbContext : DbContext
    {
        public ShopDbContext(DbContextOptions<ShopDbContext> options) : base(options) { Database.EnsureCreated(); }
    }
}
