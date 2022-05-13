using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebCrawler.Models;

namespace WebCrawler.Controllers
{
    public class BackendController : Controller
    {
        private readonly CrawlerContext DB;

        public BackendController(CrawlerContext _DB)
        {
            DB = _DB;

        }
        public async Task<IActionResult> Index()
        {
            var Announment = await DB.Announments.ToListAsync();
            var all = from m in DB.Announments select m;
            return View(all);
        }
        
        public async Task<IActionResult> Announment_Content(int id)
        {
            var content = await DB.Announments.Where(x=>x.AnnoId == id).FirstAsync();
            return View(content);
        }

        public async Task<IActionResult> Announment_Manage()
        {
            var manage_content = await DB.Announments.ToListAsync();
            return View(manage_content);
        }

        public async Task<IActionResult> Announment_Edit(int id)
        {
            var message = await DB.Announments.Where(x => x.AnnoId == id).FirstOrDefaultAsync();
            return View(message);
        }

        public async Task<IActionResult> Announment_Edit_run(int id,[Bind("AnnoId,Title,Content,Date")]Announment announment)
        {

            if (announment.Date == null)
            {
                announment.Date = DateTime.Now;
            }
            try
            {
                DB.Update(announment);
                await DB.SaveChangesAsync();
                return RedirectToAction("Announment_Manage", "Backend"); 
            }
            catch (DbUpdateConcurrencyException)
            {
                ViewBag.EditError = "修改失敗";
                return RedirectToAction("Announment_Edit", "Backend", new { id = id });
            }
            
        }
        public async Task<IActionResult> Announment_del(int id)
        {
            var message = await DB.Announments.Where(x=>x.AnnoId == id).FirstAsync();
            DB.Announments.Remove(message);
            DB.SaveChanges();
            return RedirectToAction("Announment_Manage", "Backend");
        }

        public IActionResult Announment_Create()
        {
            return View();
        }

        public async Task<IActionResult> Announment_Instert([Bind("Title,Content")]Announment announment)
        {
            announment.Date = DateTime.Now;
            DB.Add(announment);
            await DB.SaveChangesAsync();
            return RedirectToAction("Announment_Manage", "Backend");
        }
    }
}
