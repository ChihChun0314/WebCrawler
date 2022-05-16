using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Linq;
using WebCrawler.Models;
using System.Data.SqlClient;

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
        public IActionResult Login_Check(string Email,string Password)
        {
            if(Email=="admin" && Password == "aaa")
            {
                HttpContext.Session.SetString("admin", "admin");
                return RedirectToAction("Index", "Backend");
            }
            if (Email == null || Password == null)
            {
                return View("Login");
            }
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                var Check = DB.Users.Where(x => x.UEmail == Email && x.UPassword == Password).FirstOrDefault();
                if(Check != null)
                {
                    HttpContext.Session.SetInt32("UserId", Check.UId);
                    HttpContext.Session.SetString("UserName", Check.UName);
                    @ViewBag.complete = "登入成功";
                    return RedirectToAction("Index", "Backend");
                }
            }
            @ViewBag.complete = "登入失敗";
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
        public async Task<IActionResult> Register_Join(string UEmail,[Bind("UName,UEmail,UPassword,PhoneNumber")]User user)
        {
            if (DB.Users.Where(x => x.UEmail == UEmail).First() == null)
            {
                if (user.UEmail != null && user.UName != null && user.UPassword != null)
                {
                    user.Permission = "user";
                    DB.Add(user);
                    await DB.SaveChangesAsync();
                    @ViewBag.complete = "註冊成功";
                    return View("Login");
                }
                else
                {
                    @ViewBag.complete = "註冊失敗";
                    return View("Login");
                }
            }
            else
            {
                @ViewBag.complete = "Email重複註冊";
                return View("Login");
            }
        }

        public IActionResult Login_Out()
        {
            HttpContext.Session.Remove("UserId");
            HttpContext.Session.Remove("admin");
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
