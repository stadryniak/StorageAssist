using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace StorageAssist.Controllers
{
    public class StorageController : Controller
    {
        // GET
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public string AddStorage()
        {
            return "Test";
            //return RedirectToAction("Index");
        }
    }
}