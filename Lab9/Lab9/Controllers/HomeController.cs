using Lab9.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Lab9.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private ShopDbContext Context { get; set; }

        public HomeController(ShopDbContext db)
        {
            Context = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Authorization(string email, string password, string role)
        {
            if (role == "" || role == null)
            {
                return View("Authorization", "Enter a role");
            }
            if (role == "seller")
            {
                var res = Context.Sellers.ToList().Join(Context.Users.ToList(), x => x.UserId, y => y.Id, (x, y) => new { SellerId = x.Id, y.LoginPasswordId }).Join(Context.LoginsPasswords.ToList(), x => x.LoginPasswordId, y => y.Id, (x, y) => new { x.SellerId, y.Login, y.Password }).Where(x => x.Login == email && x.Password == password).ToList();
                if (res.Count == 0)
                {
                    return View("Authorization", "Not found");
                }
                else
                {
                    Response.Cookies.Append("seller", "seller-" + res.First().SellerId.ToString());
                    return View("Index");
                }
            }
            else if (role == "customer")
            {
                var res = Context.Customers.ToList().Join(Context.Users.ToList(), x => x.UserId, y => y.Id, (x, y) => new { CustomerId = x.Id, y.LoginPasswordId }).Join(Context.LoginsPasswords.ToList(), x => x.LoginPasswordId, y => y.Id, (x, y) => new { x.CustomerId, y.Login, y.Password }).Where(x => x.Login == email && x.Password == password).ToList();
                if (res.Count == 0)
                {
                    return View("Authorization", "Not found");
                }
                else
                {
                    Response.Cookies.Append("user", "customer-" + res.First().CustomerId.ToString());
                    return View("Index");
                }
            }
            return View();
        }

        [HttpGet]
        public IActionResult Authorization()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registration(string email, string password, string password2, string role, string firstname, string lastname)
        {
            if (password != password2)
            {
                return View("Authorization", "Password mismatch");
            }
            else
            {
                if (role == "seller")
                    await RegisterSellerAsync(email, password, firstname, lastname);
                else if (role == "customer")
                    await RegisterCustomerAsync(email, password, firstname, lastname);
                return View("Index");
            }
        }

        public async Task RegisterSellerAsync(string email, string password, string firstname, string lastname)
        {
            await Task.Run(() =>
            {
                int userId = GetUserId(email, password, firstname, lastname).Result;
                Context.Sellers.Add(new Seller() { UserId = userId });
                Context.SaveChanges();
            });
        }

        public async Task RegisterCustomerAsync(string email, string password, string firstname, string lastname)
        {
            await Task.Run(() =>
            {
                int userId = GetUserId(email, password, firstname, lastname).Result;
                Context.Customers.Add(new Customer() { UserId = userId });
                Context.SaveChanges();
            });
        }

        private async Task<int> GetUserId(string email, string password, string firstname, string lastname)
        {
            await Context.LoginsPasswords.AddAsync(new LoginPassword() { Login = email, Password = password });
            await Context.SaveChangesAsync();
            int loginPasswordId = Context.LoginsPasswords.OrderBy(lp => lp.Id).LastOrDefault()?.Id ?? 0;
            await Context.Users.AddAsync(new Models.User() { FirstName = firstname, LastName = lastname, LoginPasswordId = loginPasswordId });
            await Context.SaveChangesAsync();
            int userId = Context.Users.OrderBy(u => u.Id).LastOrDefault()?.Id ?? 0;
            return userId;
        }


        [HttpGet]
        public IActionResult Registration()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}