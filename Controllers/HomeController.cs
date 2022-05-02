﻿using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Linq;
using WebCrawler.Models;
using System.Data.SqlClient;

namespace WebCrawler.Controllers
{
    public class HomeController : Controller
    {
        SqlCommand com = new SqlCommand();
        SqlDataReader dr;
        SqlConnection conn = new SqlConnection();
        List<User> userinfo = new List<User>();
        private readonly CrawlerContext DB;

        public HomeController(CrawlerContext _DB)
        {
            DB = _DB;
            conn.ConnectionString = WebCrawler.Properties.Resources.ConnectionString;
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login_Check(string Email,string Password)
        {
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
                    return RedirectToAction("Index");
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
        public async Task<IActionResult> Register_Join([Bind("UName,UEmail,UPassword,PhoneNumber")]User user)
        {
            if(user.UEmail != null && user.UName != null && user.UPassword != null)
            {
                user.Permission = "user";
                DB.Add(user);
                await DB.SaveChangesAsync();
                @ViewBag.complete = "註冊成功";
                return View("Login");
            }            
            @ViewBag.complete = "註冊失敗";
            return View("Register");
        }

        public IActionResult Login_Out()
        {
            HttpContext.Session.Remove("UserId");
            return RedirectToAction("Login");
        }

        public IActionResult SetInterval()
        {
            return View("SetInterval");
        }
        
        public IActionResult ScanRecords()
        {
            fetchData();
            return View(userinfo);
        }

        private void fetchData()
        {
            if(userinfo.Count > 0)
            {
                userinfo.Clear();
            }
            try
            {
                int id = (int)HttpContext.Session.GetInt32("UserId");
                conn.Open();
                com.Connection = conn;
                com.CommandText = $"SELECT TOP (1000) [U_Email],[U_Password],[U_Name],[Phone_Number]FROM[Crawler].[dbo].[User] WHERE [U_ID]={id}";
                dr = com.ExecuteReader();
                while (dr.Read())
                {
                    userinfo.Add(new User() { UEmail = dr["U_Email"].ToString() 
                    , UPassword = dr["U_Password"].ToString()
                    , UName = dr["U_Name"].ToString()
                    , PhoneNumber = dr["Phone_Number"].ToString()
                    });
                }
                conn.Close();
            }
            catch (Exception)
            {

                throw;
            }
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
