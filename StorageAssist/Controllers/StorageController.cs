using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using StorageAssist.Models;

namespace StorageAssist.Controllers
{
    public class StorageController : Controller
    {
        private readonly AppUserContext _appUserContext;
        private readonly UserManager<ApplicationUser> _user;
        public StorageController(AppUserContext appUserContext, UserManager<ApplicationUser> user)
        {
            _appUserContext = appUserContext;
            _user = user;
        }
            // GET
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public string AddNewStorage([Bind("StorageName", "StorageType")] Storage storage)
        {
            if (ModelState.IsValid)
            {
                var userList = _appUserContext.ApplicationUser.Where(u => u.Id == _user.GetUserId(HttpContext.User))
                    .ToList();
                if (userList.Count != 1)
                {
                    return "Error";
                }
                var user = userList[0];

                var storage = new Storage();

            }
            return "Test";
            //return RedirectToAction("Index");
        }

        public string AddExistingStorage(string commonResourceId)
        {
            System.Diagnostics.Debug.WriteLine(commonResourceId);
            return "Test";
            //return RedirectToAction("Index");
        }
    }
}