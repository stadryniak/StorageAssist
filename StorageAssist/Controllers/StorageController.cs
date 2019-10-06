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
            var userList = _appUserContext.ApplicationUser.Where(u => u.Id == _user.GetUserId(HttpContext.User))
                .Include(u => u.UserCommonResource)
                    .ThenInclude(uc => uc.CommonResource)
                .ToList();
            if (userList.Count != 1 || userList[0] == null)
            {
                return View("Error");
            }

            var user = userList[0];
            return View(user);
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public string AddNewStorage([Bind("StorageId, CommonResourceId, CommonResource, OwnerId, StorageName, StorageType, Products")] Storage storage)
        {
            var userList = _appUserContext.ApplicationUser.Where(u => u.Id == _user.GetUserId(HttpContext.User))
                    .Include(u => u.UserCommonResource)
                    .ToList();
            if (userList.Count != 1)
            {
                return "Error. Try again or contact administrator.";
            }

            var user = userList[0];

            storage.OwnerId = _user.GetUserId(HttpContext.User);
            var common = new CommonResource
            {
                Storages = new List<Storage>()
                {
                    storage
                },
                Notes = null,
                OwnerId = user.Id,
            };

            var userCommon = new UserCommonResource()
            {
                CommonResource = common,
                User = user,
            };

            user.UserCommonResource.Add(userCommon);

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
            var common = new CommonResource()
            {
                OwnerId = "ja"
            };
            _appUserContext.CommonResources.Add(common);
            _appUserContext.SaveChanges();
            return "NoExceptions";
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