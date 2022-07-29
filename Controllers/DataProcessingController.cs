using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using WebCrawler.Models;
using Microsoft.EntityFrameworkCore;

namespace WebCrawler.Controllers
{
    public class DataProcessingController : Controller
    {
        private readonly CrawlerContext DB;

        public DataProcessingController(CrawlerContext _DB)
        {
            DB = _DB;
            conn.ConnectionString = WebCrawler.Properties.Resources.ConnectionString;
        }
        SqlCommand com = new SqlCommand();
        SqlDataReader dr;
        SqlConnection conn = new SqlConnection();
        List<User> userinfo = new List<User>();
        List<Crawler> crawlerinfo = new List<Crawler>();
        List<PostAnalysis> postinfo = new List<PostAnalysis>();
        List<PostAnalysis> PAnalysis = new List<PostAnalysis>();

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
            var script = @"Detect.py";
            var id = (int)HttpContext.Session.GetInt32("UserId");
            var typeID = 1; // typeId 1 = names
            //var end = "10";

            psi.Arguments = $"\"{script}\" \"{Url}\" \"{urlName}\" \"{id}\" \"{typeID}\"";

            // 3) Process configuration
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;



            var process = Process.Start(psi);
            process.WaitForExit();
            TempData["urlName"] = urlName;
            TempData["Url"] = Url;
            return View("Temp");

        }

        public IActionResult Phone()
        {
            return View();
        }

        [HttpPost]
        public IActionResult detectPhone(string urlName, string Url)
        {
            var psi = new ProcessStartInfo();
            psi.FileName = @"C:\Users\李培聖\AppData\Local\Programs\Python\Python36\python.exe";

            // 2) Provide script and arguments
            var script = @"Detect.py";
            var id = (int)HttpContext.Session.GetInt32("UserId");
            var typeID = 2; // typeId 2 = phones

            psi.Arguments = $"\"{script}\" \"{Url}\" \"{urlName}\" \"{id}\" \"{typeID}\"";

            // 3) Process configuration
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;



            var process = Process.Start(psi);
            process.WaitForExit();
            TempData["urlName"] = urlName;
            TempData["Url"] = Url;
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
                com.CommandText = $"SELECT TOP (1) [Time],[Content],[URL],[Web_name] FROM[Crawler].[dbo].[Crawler] WHERE [U_ID]={id} ORDER BY [Time] DESC";
                //  ORDER BY [Time] DESC
                dr = com.ExecuteReader();
                while (dr.Read())
                {
                    crawlerinfo.Add(new Crawler()
                    {
                        Time = (DateTime?)dr["Time"]
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
                com.CommandText = $"SELECT TOP (1) C.Time, A.Content, C.URL, C.Web_name FROM[Crawler].[dbo].[Analysis] as A INNER JOIN[Crawler].[dbo].[Crawler] as C on A.C_ID = C.C_ID where[U_ID] ={id} ORDER BY [Time] DESC";
                //  ORDER BY [Time] DESC
                dr = com.ExecuteReader();
                while (dr.Read())
                {
                    postinfo.Add(new PostAnalysis()
                    {
                        Time = (DateTime?)dr["Time"]
                    ,
                        PostContent = dr["Content"].ToString()
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

        public async Task<IActionResult> UserRecords()
        {
            if (PAnalysis.Count > 0)
            {
                PAnalysis.Clear();
            }
            int id = (int)HttpContext.Session.GetInt32("UserId");
            var message = from Crawler in DB.Crawlers
                          join Analysis in DB.Analyses
                          on Crawler.CId equals Analysis.CId
                          where Crawler.UId == id
                          select new { Crawler, Analysis };
            var ms = await message.ToListAsync();
            ms.Reverse();
            foreach (var odj in ms)
            {
                PAnalysis.Add(new PostAnalysis()
                {
                    AId = odj.Analysis.AId,
                    CId = odj.Crawler.CId,
                    TId = odj.Analysis.TId,
                    PreContent = odj.Crawler.Content,
                    PostContent = odj.Analysis.Content,
                    Time = odj.Crawler.Time,
                    Url = odj.Crawler.Url,
                    WebName = odj.Crawler.WebName
                });
            }
            return View(PAnalysis);
        }

        public async Task<IActionResult> PreContent(int id)
        {
            var analysis = await DB.Analyses.Where(x => x.AId == id).FirstOrDefaultAsync();
            var crawler = await DB.Crawlers.Where(x => x.CId == analysis.CId).FirstOrDefaultAsync();
            PostAnalysis PostAnalysis = new PostAnalysis();
            PostAnalysis.AId = analysis.AId;
            PostAnalysis.CId = crawler.CId;
            PostAnalysis.TId = analysis.TId;
            PostAnalysis.PreContent = crawler.Content;
            PostAnalysis.PostContent = analysis.Content;
            PostAnalysis.Time = crawler.Time;
            PostAnalysis.Url = crawler.Url;
            PostAnalysis.WebName = crawler.WebName;
            return View(PostAnalysis);
        }

        public async Task<IActionResult> PostContent(int id)
        {
            var analysis = await DB.Analyses.Where(x => x.AId == id).FirstOrDefaultAsync();
            var crawler = await DB.Crawlers.Where(x => x.CId == analysis.CId).FirstOrDefaultAsync();
            PostAnalysis PostAnalysis = new PostAnalysis();
            PostAnalysis.AId = analysis.AId;
            PostAnalysis.CId = crawler.CId;
            PostAnalysis.TId = analysis.TId;
            PostAnalysis.PreContent = crawler.Content;
            PostAnalysis.PostContent = analysis.Content;
            PostAnalysis.Time = crawler.Time;
            PostAnalysis.Url = crawler.Url;
            PostAnalysis.WebName = crawler.WebName;
            return View(PostAnalysis);
        }
    }
}
