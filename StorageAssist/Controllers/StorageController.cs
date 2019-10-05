using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace StorageAssist.Controllers
{
    public class StorageController : Controller
    {
        // GET
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public string AddStorage(string storageName, string storageType, string commonResourceId)
        {
            System.Diagnostics.Debug.WriteLine(storageName);
            System.Diagnostics.Debug.WriteLine(storageType);
            return "Test";
            //return RedirectToAction("Index");
        }
    }
}