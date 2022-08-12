using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebCrawler.Models;

namespace WebCrawler.Controllers
{
    public class IntervalController : Controller
    {
        private readonly CrawlerContext DB;

        public IntervalController(CrawlerContext _DB)
        {
            DB = _DB;

        }

        public IActionResult SetInterval()
        {
            return View();
        }
        public async Task<IActionResult> Set_Interval(string WebName,string Url,int time)
        {
            Interval interval = new Interval();
            DateTime date_1 = new DateTime(2022, 07, 28, 22,20,00);
            DateTime date_3 = DateTime.Now;
            switch (time)
            {
                case 1:
                    date_3 = date_3.AddDays(1);
                    break;
                case 2:
                    date_3 = date_3.AddDays(7);
                    break;
                case 3:
                    date_3 = date_3.AddMonths(1);
                    break;
                case 4:
                    date_3 = date_3.AddYears(1);
                    break;
            }

            

            interval.UId = HttpContext.Session.GetInt32("UserId");
            interval.WebName = WebName;
            interval.Url = Url;
            interval.Next = date_3;
            interval.Day = time;
            DB.Add(interval);
            await DB.SaveChangesAsync();

            //Content(((date_1 - date_2).TotalMinutes).ToString())
            @ViewBag.interval = "設定成功";
            return View("SetInterval");
        }

        public IActionResult Run_Interval()
        {
            Interval interval = new Interval();

            var a = DB.Intervals.OrderByDescending(x=>x.Next).FirstOrDefault();
            interval.Next = a.Next;
            DateTime date_1 = (DateTime)interval.Next;
            DateTime date_3 = DateTime.Now;
            var time = Math.Round((date_1 - date_3).TotalSeconds);
            if (time <= 0)
            {
                ViewBag.time = 0;
            }
            else
            {
                ViewBag.time = time;
            }
            
            return View(interval);
        }
        public IActionResult Run_DataProcessing()
        {
            Interval interval = new Interval();

            var a = DB.Intervals.OrderByDescending(x => x.Next).FirstOrDefault();
            interval.UId = a.UId;
            interval.Url = a.Url;
            interval.WebName = a.WebName;
            interval.IId = a.IId;
            interval.Next = a.Next;
            interval.Day = a.Day;
            return RedirectToAction("detectInfo","interval",interval);
        }

        public async Task<IActionResult> detectInfo(Interval interval)
        {
            var psi = new ProcessStartInfo();
            var Url = interval.Url;
            var urlName = interval.WebName;
            psi.FileName = @"C:\Users\Williamko\AppData\Local\Programs\Python\Python310\python.exe";

            // 2) Provide script and arguments
            var script = @"Detect.py";
            var id = interval.UId;
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

            DateTime date = (DateTime)interval.Next;
            switch (interval.Day)
            {
                case 1:
                    date = date.AddDays(1);
                    break;
                case 2:
                    date = date.AddDays(7);
                    break;
                case 3:
                    date = date.AddMonths(1);
                    break;
                case 4:
                    date = date.AddYears(1);
                    break;
            }

            interval.Next = date;
            DB.Update(interval);
            await DB.SaveChangesAsync();

            return RedirectToAction("Run_Interval", "interval", interval);

        }
        public async Task<IActionResult> User_Interval()
        {
            var User = HttpContext.Session.GetInt32("UserId");
            var User_Interval = await DB.Intervals.Where(x => x.UId == User).ToListAsync();
            return View(User_Interval);
        }

        public IActionResult User_Interval_Edit(int id)
        {
            Interval interval1 = new Interval();
            interval1 = DB.Intervals.Where(x => x.IId == id).FirstOrDefault();
            return View(interval1);
        }

        public async Task<IActionResult> User_Interval_Edit_Run( [Bind("IId,WebName,Url,Day")] Interval interval)
        {
            try
            {
                switch (interval.Day)
                {
                    case 1:
                        interval.Next = DateTime.Now.AddDays(1);
                        break ;
                    case 2:
                        interval.Next = DateTime.Now.AddDays(7);
                        break;
                    case 3:
                        interval.Next = DateTime.Now.AddMonths(1);
                        break;
                    case 4:
                        interval.Next = DateTime.Now.AddYears(1);
                        break;
                }   
                interval.UId = HttpContext.Session.GetInt32("UserId");
                DB.Update(interval);
                await DB.SaveChangesAsync();
                return RedirectToAction("User_Interval", "interval"); ;
            }
            catch (DbUpdateConcurrencyException)
            {
                ViewBag.EditError = "修改失敗";
                return RedirectToAction("User_Interval", "interval");
            }
        }

        public async Task<IActionResult> User_Interval_del(int id)
        {
            var message = await DB.Intervals.Where(x => x.IId == id).FirstAsync();
            DB.Intervals.Remove(message);
            DB.SaveChanges();
       
            return RedirectToAction("User_Interval", "interval" );
        }
        
    }
}
