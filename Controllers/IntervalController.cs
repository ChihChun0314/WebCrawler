using Microsoft.AspNetCore.Mvc;
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

            Interval  interval = new Interval();

            interval.UId = HttpContext.Session.GetInt32("UserId");
            interval.WebName = WebName;
            interval.Url = Url;
            interval.Next = date_3;
            DB.Add(interval);
            await DB.SaveChangesAsync();

            //Content(((date_1 - date_2).TotalMinutes).ToString())
            @ViewBag.interval = "設定成功";
            return View("SetInterval");
        }
    }
}
