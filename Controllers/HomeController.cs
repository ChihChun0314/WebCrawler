using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Linq;
using WebCrawler.Models;

namespace WebCrawler.Controllers
{
    public class HomeController : Controller
    {
        private readonly CrawlerContext DB;

        public HomeController(CrawlerContext _DB)
        {
            DB = _DB;
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login_Check([Bind("Account,Password")]User user)
        {
            if (user == null)
            {
                return View("Login");
            }
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                var Check = DB.Users.Where(x => x.Account == user.Account && x.Password == user.Password).FirstOrDefault();
                if(Check != null)
                {
                    HttpContext.Session.SetInt32("UserId", Check.UserId);
                    return RedirectToAction("Index");
                }
            }
            return View("Login");
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register_Join([Bind("Name,Account,Password,Tel")]User user)
        {
            if(user.Account != null && user.Name != null && user.Password != null)
            {
                DB.Add(user);
                await DB.SaveChangesAsync();
                return View("Login");
            }            
            @ViewBag.complete = "資料填寫不完全";
            return View("Register");
        }

        public IActionResult Login_Out()
        {
            HttpContext.Session.Remove("UserId");
            return RedirectToAction("Login");
        }
        public IActionResult test(string Url)
        {
            var psi = new ProcessStartInfo();
            psi.FileName = @"C:\Users\Williamko\AppData\Local\Programs\Python\Python310\python.exe";

            // 2) Provide script and arguments
            var script = @"C:\Users\Williamko\Desktop\Core\C#\WebCrawler\Web.py";
            var start = Url;
            //var end = "10";

            psi.Arguments = $"\"{script}\" \"{start}\"";

            // 3) Process configuration
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;



            var process = Process.Start(psi);


            return Content(Url);
        }
    }
}
