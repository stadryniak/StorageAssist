using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using StorageAssist.Models;

namespace StorageAssist.Controllers
{
    public class StorageController : Controller
    {
        private readonly AppUserContext _appUserContext;
        private readonly UserManager<IdentityUser> _user;
        public StorageController(AppUserContext appUserContext, UserManager<IdentityUser> user)
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
        public string AddNewStorage([Bind("StorageId, CommonResourceId, CommonResource, OwnerId, StorageName, StorageType, Products")] Storage storage)
        {
            var user = _appUserContext.ApplicationUser.Where(u => u.Id == _user.GetUserId(HttpContext.User)).ToList()[0];

            var common = new CommonResource
            {
                Notes = null,
                OwnerId = user.Id
            };
            common.Storages.Add(storage);

            var join = new UserCommonResource
            {
                UserCommonResourceId = RandomString(39),
                UserId = user.Id,
                User = user,
                CommonResourceId = common.CommonResourceId,
                CommonResource = common
            };
            common.UserCommonResource.Add(join);

            user.UserCommonResource.Add(join);
            _appUserContext.ApplicationUser.Update(user);
            _appUserContext.SaveChanges();
            return "Added. I believe.";
            /*
            //                                Should work but don't. 
            TryValidateModel(storage);
            if (ModelState.IsValid)
            {
                _appUserContext.Update(user);

            }
            return "Error. Model invalid";
            */
            //return RedirectToAction("Index");
        }

        public string AddExistingStorage(string commonResourceId)
        {
            System.Diagnostics.Debug.WriteLine(commonResourceId);
            return "Test";
            //return RedirectToAction("Index");
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789qwertyuiopasdfghjklzxcvbnm-";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}