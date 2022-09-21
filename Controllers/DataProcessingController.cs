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
        List<PostPreview> postinfo = new List<PostPreview>();
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
            //var typeID = 1; // typeId 1 = names
            //var end = "10";

            psi.Arguments = $"\"{script}\" \"{Url}\" \"{urlName}\" \"{id}\"";

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
                com.CommandText = $"SELECT TOP (1) [C_ID],[Time],[Content],[URL],[Web_name] FROM[Crawler].[dbo].[Crawler] WHERE [U_ID]={id} ORDER BY [Time] DESC";
                //  ORDER BY [Time] DESC
                dr = com.ExecuteReader();
                while (dr.Read())
                {
                    crawlerinfo.Add(new Crawler()
                    {
                        CId = (int)dr["C_ID"]
                    ,
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
            int id = (int)HttpContext.Session.GetInt32("UserId");
            var message = from Crawler in DB.Crawlers
                          where Crawler.UId == id
                          select new { Crawler};
            var ms = message.ToList();
            ms.Reverse();

            int cid = ms[0].Crawler.CId;
            var crawler = DB.Crawlers.Where(x => x.CId == cid).FirstOrDefault();
            var name = DB.Analyses.Where(x => x.CId == cid && x.TId == 1).FirstOrDefault();
            var phone = DB.Analyses.Where(x => x.CId == cid && x.TId == 2).FirstOrDefault();
            var email = DB.Analyses.Where(x => x.CId == cid && x.TId == 3).FirstOrDefault();
            var address = DB.Analyses.Where(x => x.CId == cid && x.TId == 4).FirstOrDefault();

            postinfo.Add(new PostPreview()
            {
                Time = crawler.Time,
                Url = crawler.Url,
                WebName = crawler.WebName,
                Name = name.Content,
                Phone = phone.Content,
                Email = email.Content,
                Address = address.Content

            });



        }

        public async Task<IActionResult> UserRecords_Index()
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
            var qq = 1;
            foreach (var odj in ms)
            {
                if (qq != odj.Crawler.CId)
                {
                    qq = odj.Crawler.CId;

                    PAnalysis.Add(new PostAnalysis()
                    {
                        AId = odj.Analysis.AId,
                        CId = odj.Crawler.CId,
                        TId = odj.Analysis.TId,
                        PreContent = odj.Crawler.Content,
                        PostContent = odj.Analysis.Content,
                        Time = odj.Crawler.Time,
                        Url = odj.Crawler.Url,
                        WebName = odj.Crawler.WebName,
                        Count = odj.Analysis.Count
                    });
                }

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
            PostAnalysis.Count = analysis.Count;
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
            PostAnalysis.Count = analysis.Count;
            return View(PostAnalysis);
        }
        public async Task<IActionResult> UserRecords_class(int id)
        {
            var name = await DB.Analyses.Where(x => x.CId == id && x.TId == 1).FirstOrDefaultAsync();
            var phone = await DB.Analyses.Where(x => x.CId == id && x.TId == 2).FirstOrDefaultAsync();
            var email = await DB.Analyses.Where(x => x.CId == id && x.TId == 3).FirstOrDefaultAsync();
            var address = await DB.Analyses.Where(x => x.CId == id && x.TId == 4).FirstOrDefaultAsync();
            Statistics Statistics_data = new Statistics();
            if (name != null || phone != null || email != null || address != null)
            {
                Statistics_data.CId = id;
                Statistics_data.name = (int)name.Count;
                Statistics_data.phone = (int)phone.Count;
                Statistics_data.email = (int)email.Count;
                Statistics_data.address = (int)address.Count;

                TempData["name_count"] = name.Count;
                TempData["phone_count"] = phone.Count;
                TempData["email_count"] = email.Count;
                TempData["address_count"] = address.Count;

                TempData["name_id"] = (int)name.AId;
                TempData["phone_id"] = (int)phone.AId;
                TempData["email_id"] = (int)email.AId;
                TempData["address_id"] = (int)address.AId;
            }
            else
            {
                Statistics_data.name = id;
                Statistics_data.phone = id;
                Statistics_data.email = id;
                Statistics_data.address = id;
            }
            TempData["user_id"] = id;
            return View(Statistics_data);
        }

        public IActionResult Excel()
        {
            return View();
        }

        public IActionResult export(int id)
        {
            var psi = new ProcessStartInfo();
            psi.FileName = @"C:\Users\Williamko\AppData\Local\Programs\Python\Python310\python.exe";

            // 2) Provide script and arguments
            var script = @"export.py";
            //var typeID = 1; // typeId 1 = names
            //var end = "10";

            psi.Arguments = $"\"{script}\" \"{id}\"";

            // 3) Process configuration
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;



            var process = Process.Start(psi);
            process.WaitForExit();
            return Content("Done");

        }
    }
}
