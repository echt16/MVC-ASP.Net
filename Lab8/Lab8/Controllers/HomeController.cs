using Lab8.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Net;

namespace Lab8.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;

            /*List<Fraction> fractions = new List<Fraction>()
            {
                new(){ Numerator = 1, Denominator = 2 },
                new(){ Numerator = 10, Denominator = 9 },
                new(){ Numerator = 15, Denominator = 3 },
                new(){ Numerator = 123, Denominator = 5 },
                new(){ Numerator = 9, Denominator = 7 },
            };
            string json = JsonConvert.SerializeObject(fractions);
            using(StreamWriter sw = new StreamWriter("Fractions.txt", false, System.Text.Encoding.Unicode))
            {
                sw.Write(json);
            }*/
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Actions(int numerator1, int numerator2, int denominator1, int denominator2, string action, string reduce)
        {
            List<Fraction> fractions = GetFractions(numerator1, numerator2, denominator1, denominator2, action);
            if (reduce != null && reduce == "on")
            {
                fractions[2].Reduce();
            }
            return View(fractions);
        }

        [HttpPost]
        public IActionResult Reduce(int numerator, int denominator)
        {
            List<Fraction> fractions = new List<Fraction>() { new Fraction() { Denominator = denominator, Numerator = numerator } };
            fractions[0].Reduce();
            return View("Actions", fractions);
        }

        [HttpGet]
        public IActionResult FromFile(string src)
        {
            string json = "";
            List<Fraction> fractions = new List<Fraction>();
            if (src != null && src != "" && src.EndsWith(".txt"))
            {
                using (StreamReader sr = new StreamReader(src))
                {
                    json = sr.ReadToEnd();
                }
                fractions = JsonConvert.DeserializeObject<List<Fraction>>(json);
            }
            return View(fractions);
        }

        public List<Fraction> GetFractions(int numerator1, int numerator2, int denominator1, int denominator2, string action)
        {
            List<Fraction> fractions = new List<Fraction>() {
                new Fraction() { Numerator = numerator1, Denominator = denominator1 },
                new Fraction() { Numerator = numerator2, Denominator = denominator2 }
            };
            switch (action)
            {
                case "+":
                    {
                        if (fractions[0].Denominator != 0 && fractions[1].Denominator != 0)
                        {
                            fractions.Add(fractions[0] + fractions[1]);
                        }
                    }
                    break;
                case "-":
                    {
                        if (fractions[0].Denominator != 0 && fractions[1].Denominator != 0)
                        {
                            fractions.Add(fractions[0] - fractions[1]);
                        }
                    }
                    break;
                case "*":
                    {
                        if (fractions[0].Denominator != 0 && fractions[1].Denominator != 0)
                        {
                            fractions.Add(fractions[0] * fractions[1]);
                        }
                    }
                    break;
                case "/":
                    {
                        if (fractions[0].Denominator != 0 && fractions[1].Denominator != 0 && fractions[1].Numerator != 0)
                        {
                            fractions.Add(fractions[0] / fractions[1]);
                        }
                    }
                    break;
            }
            return fractions;
        }
        [HttpGet]
        public IActionResult Cars(string src, string actionCar)
        {
            List<Car>? cars = null;
            if (actionCar == "save" && src != null && src.Length > 0 && src.EndsWith(".txt"))
            {
                using (StreamWriter sw = new StreamWriter(src, false, System.Text.Encoding.Unicode))
                {
                    cars = GetCars();
                    sw.Write(JsonConvert.SerializeObject(cars));
                }
            }
            else
            {
                string json = "";
                if (src != null && src.Length > 0 && src.EndsWith(".txt"))
                {
                    using (StreamReader sr = new StreamReader(src, System.Text.Encoding.Unicode))
                    {
                        json = sr.ReadToEnd();
                    }
                }
                cars = JsonConvert.DeserializeObject<List<Car>>(json);
                if (cars is null)
                    cars = GetCars();
                else
                {
                    SetCarsCookie(cars);
                }
            }
            return View(cars);
        }

        public void SetCarsCookie(List<Car> cars)
        {
            Response.Cookies.Append("cars", JsonConvert.SerializeObject(cars), new CookieOptions() { Expires = (DateTimeOffset.Now.AddDays(1)) });
        }

        [HttpPost]
        public IActionResult PutCar(int id, string action)
        {
            if(action == "add")
            {
                return View(new Car());
            }
            List<Car> cars = GetCars();
            Car car = cars.FirstOrDefault(x => x.Id == id);
            return View(car);
        }
        [HttpPost]
        public IActionResult PutCurrentCar(int id, string name, string color, string publisher, int year, int engine, string action)
        {
            List<Car> cars = GetCars();
            int index = cars.FindIndex(x => x.Id == id);
            if (action == "delete")
            {
                cars.RemoveAt(index);
            }
            else
            {
                cars[index].Name = name;
                cars[index].Color = color;
                cars[index].Publisher = publisher;
                cars[index].Year = year;
                cars[index].EngineCapacity = engine;
            }
            SetCarsCookie(cars);
            return View("Cars", cars);
        }

        public IActionResult AddCar(string name, string color, string publisher, int year, int engine)
        {
            List<Car> cars = GetCars();
            cars.Add(new Car()
            {
                Name = name,
                Color = color,
                Publisher = publisher,
                Year = year,
                EngineCapacity = engine
            });
            List<int> ids = cars.Select(x => x.Id).ToList();
            int id = 0;
            while (ids.Contains(id)){
                id++;
            }
            cars.Last().Id = id;
            SetCarsCookie(cars);
            return View("Cars", cars);
        }

        public List<Car>? GetCars()
        {
            string cookie = Request.Cookies["cars"];
            if (cookie != null && cookie != "")
            {
                return JsonConvert.DeserializeObject<List<Car>>(cookie);
            }
            return new List<Car>();
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
