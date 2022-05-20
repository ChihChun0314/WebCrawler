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
        List<PostAnalysis> postinfo = new List<PostAnalysis>();
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
        public IActionResult PostAnalysis()
        {
            getPostData();
            return View(postinfo);

        }

        public IActionResult Detect()
        {
            return View();
        }

        [HttpPost]
        public IActionResult detectInfo(string urlName, string Url)
        {
            var psi = new ProcessStartInfo();
            psi.FileName = @"C:\Users\李培聖\AppData\Local\Programs\Python\Python36\python.exe";

            // 2) Provide script and arguments
            var script = @"C:\Users\李培聖\Desktop\Crawler\WebCrawler\Detect.py";
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

        public IActionResult show2()
        {
            getPostData();
            return RedirectToAction("PostAnalysis", "DataProcessing");
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
                com.CommandText = $"SELECT TOP (1) [C_ID],[Content],[URL],[Web_name] FROM[Crawler].[dbo].[Crawler] WHERE [U_ID]={id} ORDER BY [Time] DESC";
                //  ORDER BY [Time] DESC
                dr = com.ExecuteReader();
                while (dr.Read())
                {
                    crawlerinfo.Add(new Crawler()
                    {
                        CId = (int)dr["C_ID"]
                    ,
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

        private void getPostData()
        {
            try
            {
                int id = (int)HttpContext.Session.GetInt32("UserId");
                conn.Open();
                com.Connection = conn;
                com.CommandText = $"SELECT TOP (1) A.C_ID, A.Content, C.URL, C.Web_name FROM[Crawler].[dbo].[Analysis] as A INNER JOIN[Crawler].[dbo].[Crawler] as C on A.C_ID = C.C_ID where[U_ID] ={id} ORDER BY [Time] DESC";
                //  ORDER BY [Time] DESC
                dr = com.ExecuteReader();
                while (dr.Read())
                {
                    postinfo.Add(new PostAnalysis()
                    {
                        CId = ((int?)dr["C_ID"])
                    ,
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
