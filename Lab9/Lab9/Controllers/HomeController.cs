using Lab9.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

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
            ProductViewModel model = new ProductViewModel()
            {
                Products = Context.Products.ToList(),
                Kinds = Context.Kinds.ToList(),
                Categories = Context.Categories.ToList(),
            };
            return View(model);
        }

        /*[HttpPost]
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
                    Response.Cookies.Append("user", "seller-" + res.First().SellerId.ToString());
                    ProductViewModel model = new ProductViewModel()
                    {
                        Products = Context.Products.ToList(),
                        Kinds = Context.Kinds.ToList(),
                        Categories = Context.Categories.ToList(),
                    };
                    return View("Index", model);
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
                    ProductViewModel model = new ProductViewModel()
                    {
                        Products = Context.Products.ToList(),
                        Kinds = Context.Kinds.ToList(),
                        Categories = Context.Categories.ToList(),
                    };
                    return View("Index", model);
                }
            }
            return View();
        }*/

        [HttpPost]
        public IActionResult Authorization(string email, string password, string role)
        {
            if (role == "" || role == null)
            {
                return View("Authorization", "Enter a role");
            }
            if (role == "seller")
            {
                LoginPassword loginPassword = Context.LoginsPasswords.FirstOrDefault(x => x.Login == email && x.Password == password);
                if (loginPassword == null)
                {
                    return View("Authorization", "Not found");
                }
                else
                {
                    int userId = Context.Users.First(x => x.LoginPasswordId == loginPassword.Id).Id;
                    Seller seller = Context.Sellers.FirstOrDefault(x => x.UserId == userId);
                    if (seller != null)
                    {
                        var token = GenerateToken(seller.Id, "seller");
                        return Ok(new { token });
                    }
                    else
                        return View("Authorization", "Not found");
                }
            }
            else if (role == "customer")
            {
                LoginPassword loginPassword = Context.LoginsPasswords.FirstOrDefault(x => x.Login == email && x.Password == password);
                if (loginPassword == null)
                {
                    return View("Authorization", "Not found");
                }
                else
                {
                    int userId = Context.Users.First(x => x.LoginPasswordId == loginPassword.Id).Id;
                    Customer customer = Context.Customers.FirstOrDefault(x => x.UserId == userId);
                    if (customer != null)
                    {
                        var token = GenerateToken(customer.Id, "customer");
                        return Ok(new { token });
                    }
                    else
                        return View("Authorization", "Not found");
                }
            }
            return View("Authorization", "Error");
        }

        private string GenerateToken(int userId, string role)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(GenerateRandomSecretKey(32));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public static string GenerateRandomSecretKey(int length)
        {
            const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            StringBuilder stringBuilder = new StringBuilder();
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] uintBuffer = new byte[sizeof(uint)];

                while (length-- > 0)
                {
                    rng.GetBytes(uintBuffer);
                    uint num = BitConverter.ToUInt32(uintBuffer, 0);
                    stringBuilder.Append(validChars[(int)(num % (uint)validChars.Length)]);
                }
            }
            return stringBuilder.ToString();
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
                return View("Registration", "Password mismatch");
            }
            if (Context.LoginsPasswords.Select(x => x.Login).FirstOrDefault(x => x == email) != null)
            {
                return View("Registration", "Email already exists");
            }
            else
            {
                if (role == "seller")
                    await RegisterSellerAsync(email, password, firstname, lastname);
                else if (role == "customer")
                    await RegisterCustomerAsync(email, password, firstname, lastname);
                ProductViewModel model = null;
                bool b = false;
                do
                {
                    b = Monitor.TryEnter(Context, 5000);
                    model = new ProductViewModel()
                    {
                        Products = Context.Products.ToList(),
                        Kinds = Context.Kinds.ToList(),
                        Categories = Context.Categories.ToList(),
                    };
                } while (!b);
                return View("Index", model);
            }
        }

        public async Task RegisterSellerAsync(string email, string password, string firstname, string lastname)
        {
            await Task.Run(() =>
            {
                bool b = false;
                do
                {
                    b = Monitor.TryEnter(Context, 5000);
                    int userId = GetUserId(email, password, firstname, lastname).Result;
                    Context.Sellers.Add(new Seller() { UserId = userId });
                    Context.SaveChanges();
                } while (!b);
            });
        }

        public async Task RegisterCustomerAsync(string email, string password, string firstname, string lastname)
        {
            await Task.Run(() =>
            {
                bool b = false;
                do
                {
                    b = Monitor.TryEnter(Context, 5000);
                    int userId = GetUserId(email, password, firstname, lastname).Result;
                    Context.Customers.Add(new Customer() { UserId = userId });
                    Context.SaveChanges();
                } while (!b);
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

        public IActionResult AddToCart(int productId)
        {
            string? userCookie = Request.Cookies["user"];
            if (userCookie == null || userCookie == "")
            {
                return View("Authorization");
            }
            if (userCookie.StartsWith("seller"))
            {
                return View("ErrorMessage", "Register as customer");
            }
            int customerId = int.Parse(userCookie.Split("-")[1]);
            _ = AddToAddedToCartAsync(productId, customerId);
            ProductViewModel model = new ProductViewModel()
            {
                Products = Context.Products.ToList(),
                Kinds = Context.Kinds.ToList(),
                Categories = Context.Categories.ToList(),
            };
            return View("Index", model);
        }

        public async Task AddToAddedToCartAsync(int productId, int customerId)
        {
            Context.AddedToCartProducts.Add(new AddedToCartProduct()
            {
                CustomerId = customerId,
                ProductId = productId,
                DateTime = DateTime.Now
            });
            await Context.SaveChangesAsync();
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

        [HttpGet]
        [CustomerAuthorize]
        public IActionResult ProductDetails(int productId)
        {
            ProductDetailsViewModel model = new ProductDetailsViewModel()
            {
                Product = Context.Products.First(x => x.Id == productId),
                Photos = Context.Photos.Where(x => x.ProductId == productId).ToList()
            };

            Kind kind = Context.Kinds.First(x => x.Id == model.Product.KindId);
            string category = Context.Categories.First(x => x.Id == kind.CategoryId).Name;
            string kindStr = kind.Name;

            User seller = Context.Users.First(x => x.Id == Context.Sellers.First(x => x.Id == model.Product.SellerId).UserId);

            model.Seller = seller;

            model.Kind = kindStr;
            model.Category = category;

            return View(model);
        }
    }
}

public class CustomerAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (!context.HttpContext.User.Identity.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (!context.HttpContext.User.IsInRole("customer"))
        {
            context.Result = new ForbidResult();
            return;
        }
    }
}