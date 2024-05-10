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
using Newtonsoft.Json;

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
                        var obj = AuthorizationJWT.GenerateToken(seller.Id, "seller");
                        string token = obj[0] as string;
                        byte[] key = obj[1] as byte[];
                        CookieOptions co = new CookieOptions();
                        co.Expires = DateTimeOffset.Now.AddDays(1);
                        Response.Cookies.Append("token", token, co);
                        Response.Cookies.Append("key", JsonConvert.SerializeObject(key), co);
                        ProductViewModel model = new ProductViewModel()
                        {
                            Products = Context.Products.ToList(),
                            Kinds = Context.Kinds.ToList(),
                            Categories = Context.Categories.ToList(),
                        };
                        return View("Index", model);
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
                        var obj = AuthorizationJWT.GenerateToken(customer.Id, "customer");
                        string token = obj[0] as string;
                        byte[] key = obj[1] as byte[];
                        CookieOptions co = new CookieOptions();
                        co.Expires = DateTimeOffset.Now.AddDays(1);
                        Response.Cookies.Append("token", token, co);
                        Response.Cookies.Append("key", JsonConvert.SerializeObject(key), co);
                        ProductViewModel model = new ProductViewModel()
                        {
                            Products = Context.Products.ToList(),
                            Kinds = Context.Kinds.ToList(),
                            Categories = Context.Categories.ToList(),
                        };

                        return View("Index", model);
                    }
                    else
                        return View("Authorization", "Not found");
                }
            }
            return View("Authorization", "Error");
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
                return View("Authorization");
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
                    if (b)
                        Monitor.Exit(Context);
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
                    if (b)
                        Monitor.Exit(Context);
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

        public async Task<IActionResult> AddToCart(int productId)
        {
            if (!CheckAuthorization("customer"))
            {
                return View("ErrorMessage", "Register as customer");
            }
            int customerId = AuthorizationJWT.GetIdOfCurrentUser(Request.Cookies["token"], Request.Cookies["key"]);
            if (customerId == -1)
            {
                return View("ErrorMessage", "Register as customer");
            }
            await AddToAddedToCartAsync(productId, customerId);
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
            lock (Context)
            {
                Context.AddedToCartProducts.Add(new AddedToCartProduct()
                {
                    CustomerId = customerId,
                    ProductId = productId,
                    DateTime = DateTime.Now
                });
            }
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
        public IActionResult ProductDetails(int productId)
        {
            if (!CheckAuthorization("customer"))
            {
                return View("ErrorMessage", "Register as customer!");
            }
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

        [HttpGet]
        public IActionResult CustomerCart()
        {
            if (!CheckAuthorization("customer"))
            {
                return View("ErrorMessage", "Register as customer!");
            }
            int customerId = AuthorizationJWT.GetIdOfCurrentUser(Request.Cookies["token"], Request.Cookies["key"]);
            CartViewModal model = new CartViewModal()
            {
                AddedToCartProducts = Context.AddedToCartProducts.Where(x => x.CustomerId == customerId).ToList()
            };
            for (int i = 0; i < model.AddedToCartProducts.Count; i++)
            {
                Product product = Context.Products.First(x => x.Id == model.AddedToCartProducts[i].ProductId);
                model.Products.Add(product);
                Seller seller = Context.Sellers.First(x => x.Id == product.SellerId);
                User user = Context.Users.First(x => x.Id == seller.UserId);
                model.Sellers.Add(user);
                model.IsConsired.Add(true);
            }
            model.Sum = model.Products.Select(x => x.Price).Sum();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CustomerCart(int[] isConsired, string hiddenInput)
        {
            int customerId = AuthorizationJWT.GetIdOfCurrentUser(Request.Cookies["token"], Request.Cookies["key"]);
            if (hiddenInput == "Refresh")
            {
                CartViewModal model = new CartViewModal()
                {
                    AddedToCartProducts = Context.AddedToCartProducts.Where(x => x.CustomerId == customerId).ToList()
                };
                double sum = 0;
                for (int i = 0; i < model.AddedToCartProducts.Count; i++)
                {
                    Product product = Context.Products.First(x => x.Id == model.AddedToCartProducts[i].ProductId);
                    model.Products.Add(product);
                    Seller seller = Context.Sellers.First(x => x.Id == product.SellerId);
                    User user = Context.Users.First(x => x.Id == seller.UserId);
                    model.Sellers.Add(user);

                    if (isConsired.Contains(model.AddedToCartProducts[i].Id))
                    {
                        model.IsConsired.Add(true);
                        sum += product.Price;
                    }
                    else
                    {
                        model.IsConsired.Add(false);
                    }
                }
                model.Sum = sum;
                return View(model);
            }
            else if (hiddenInput == "Checkout")
            {
                List<AddedToCartProduct> productsInDeals = Context.AddedToCartProducts.Where(x => isConsired.Contains(x.Id)).ToList();
                foreach (var item in productsInDeals)
                {
                    Context.Deals.Add(new Deal()
                    {
                        CustomerId = item.CustomerId,
                        DateTime = DateTime.Now,
                        IsCompleted = true,
                        ProductId = item.ProductId
                    });
                    Context.AddedToCartProducts.Remove(item);
                }
                await Context.SaveChangesAsync();
                CartViewModal model = new CartViewModal()
                {
                    AddedToCartProducts = Context.AddedToCartProducts.Where(x => x.CustomerId == customerId).ToList()
                };
                for (int i = 0; i < model.AddedToCartProducts.Count; i++)
                {
                    Product product = Context.Products.First(x => x.Id == model.AddedToCartProducts[i].ProductId);
                    model.Products.Add(product);
                    Seller seller = Context.Sellers.First(x => x.Id == product.SellerId);
                    User user = Context.Users.First(x => x.Id == seller.UserId);
                    model.Sellers.Add(user);
                    model.IsConsired.Add(false);
                }
                model.Sum = 0;
                return View(model);
            }
            return View("ErrorMessage", "Error");
        }

        public IActionResult DeleteProductFromCart(int productId)
        {
            Context.AddedToCartProducts.Remove(Context.AddedToCartProducts.First(x => x.Id == productId));
            Context.SaveChanges();
            int customerId = AuthorizationJWT.GetIdOfCurrentUser(Request.Cookies["token"], Request.Cookies["key"]);
            CartViewModal model = new CartViewModal()
            {
                AddedToCartProducts = Context.AddedToCartProducts.Where(x => x.CustomerId == customerId).ToList()
            };
            for (int i = 0; i < model.AddedToCartProducts.Count; i++)
            {
                Product product = Context.Products.First(x => x.Id == model.AddedToCartProducts[i].ProductId);
                model.Products.Add(product);
                Seller seller = Context.Sellers.First(x => x.Id == product.SellerId);
                User user = Context.Users.First(x => x.Id == seller.UserId);
                model.Sellers.Add(user);
                model.IsConsired.Add(true);
            }
            model.Sum = model.Products.Select(x => x.Price).Sum();
            return View("CustomerCart", model);
        }

        [HttpGet]
        public IActionResult CustomerDeals()
        {
            if (!CheckAuthorization("customer"))
            {
                return View("ErrorMessage", "Register as customer!");
            }
            int customerId = AuthorizationJWT.GetIdOfCurrentUser(Request.Cookies["token"], Request.Cookies["key"]);
            DealsViewModal model = new DealsViewModal()
            {
                Deals = Context.Deals.Where(x => x.CustomerId == customerId).ToList()
            };
            for (int i = 0; i < model.Deals.Count; i++)
            {
                Product product = Context.Products.First(x => x.Id == model.Deals[i].ProductId);
                model.Products.Add(product);
                Seller seller = Context.Sellers.First(x => x.Id == product.SellerId);
                User user = Context.Users.First(x => x.Id == seller.UserId);
                model.Sellers.Add(user);
            }
            model.Sum = model.Products.Select(x => x.Price).Sum();
            return View(model);
        }

        public IActionResult Account()
        {
            UserView umodel = AuthorizationJWT.GetCurrentUser(Request.Cookies["token"], Request.Cookies["key"]);
            if (umodel == null)
                return View("ErrorMessage", "Register!");
            AccountViewModel model = new AccountViewModel();
            User user = null;
            if (umodel.Role == "customer")
            {
                user = Context.Users.First(x => x.Id == Context.Customers.First(x => x.Id == umodel.Id).UserId);
            }
            else if (umodel.Role == "seller")
            {
                user = Context.Users.First(x => x.Id == Context.Sellers.First(x => x.Id == umodel.Id).UserId);
            }
            model.Firstname = user.FirstName;
            model.Lastname = user.LastName;
            LoginPassword lp = Context.LoginsPasswords.First(x => x.Id == user.LoginPasswordId);
            model.Login = lp.Login;
            model.Password = lp.Password;
            return View(model);
        }

        private bool CheckAuthorization(string role)
        {
            string tokenStr = Request.Cookies["token"];
            string keyStr = Request.Cookies["key"];
            return AuthorizationJWT.CheckAuthorization(tokenStr, keyStr, role);
        }


    }
}