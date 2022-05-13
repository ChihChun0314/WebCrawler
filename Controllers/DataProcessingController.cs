using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using WebCrawler.Models;

namespace WebCrawler.Controllers
{
    public class DataProcessingController : Controller
    {
        SqlCommand com = new SqlCommand();
        SqlDataReader dr;
        SqlConnection conn = new SqlConnection();
        List<User> userinfo = new List<User>();
        List<Crawler> crawlerinfo = new List<Crawler>();
        public DataProcessingController()
        {
            conn.ConnectionString = WebCrawler.Properties.Resources.ConnectionString;
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

        public IActionResult PreAnalysis()
        {
            getCrawlerData();
            return View(crawlerinfo);

        }

        public IActionResult Detect()
        {
            return View();
        }

        [HttpPost]
        public IActionResult detectInfo(string urlName, string Url)
        {
            var psi = new ProcessStartInfo();
            psi.FileName = @"C:\Users\jason\AppData\Local\Programs\Python\Python310\python.exe";

            // 2) Provide script and arguments
            var script = @"D:\雲科110-2\ASP\donet-test\test7\2022_05_01\Detect.py";
            var id = (int)HttpContext.Session.GetInt32("UserId");
            //var end = "10";

            psi.Arguments = $"\"{script}\" \"{Url}\" \"{urlName}\" \"{id}\"";

            // 3) Process configuration
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;



            var process = Process.Start(psi);
            process.WaitForExit();
            return View("Temp");

        }

        [HttpPost]
        public IActionResult show()
        {
            getCrawlerData();
            return RedirectToAction("PreAnalysis", "DataProcessing");
        }

        private void fetchData()
        {
            if (userinfo.Count > 0)
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
                    userinfo.Add(new User()
                    {
                        UEmail = dr["U_Email"].ToString()
                    ,
                        UPassword = dr["U_Password"].ToString()
                    ,
                        UName = dr["U_Name"].ToString()
                    ,
                        PhoneNumber = dr["Phone_Number"].ToString()
                    });
                }
                conn.Close();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void getCrawlerData()
        {
            try
            {
                int id = (int)HttpContext.Session.GetInt32("UserId");
                conn.Open();
                com.Connection = conn;
                com.CommandText = $"SELECT TOP (1) [Content],[URL],[Web_name] FROM[Crawler].[dbo].[Crawler] WHERE [U_ID]={id} ORDER BY [Time] DESC";
                //  ORDER BY [Time] DESC
                dr = com.ExecuteReader();
                while (dr.Read())
                {
                    crawlerinfo.Add(new Crawler()
                    {
                        Content = dr["Content"].ToString()
                    ,
                        Url = dr["URL"].ToString()
                    ,
                        WebName = dr["Web_name"].ToString()
                    });
                }
                conn.Close();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
