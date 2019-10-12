﻿using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
                        .ThenInclude(c => c.Storages)
                            .ThenInclude(s => s.Products)
                .Include(u => u.UserCommonResource)
                    .ThenInclude(uc => uc.CommonResource)
                        .ThenInclude(c => c.Notes)
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
        public async Task<IActionResult> AddNewStorage([Bind("StorageId, CommonResourceId, CommonResource, OwnerId, StorageName, StorageType, Products")] Storage storage, string commonResourceId, string commonResourceName)
        {
            // Probably overcomplicated, split into functions at some point 
            if (string.IsNullOrEmpty(storage.StorageName) || (string.IsNullOrEmpty(commonResourceId) && string.IsNullOrEmpty(commonResourceName)))
            {
                var error = new ErrorViewModel();
                return RedirectToAction("Index", "Error", error);
            }
            //get user and she's/he's CommonResources from database
            var userList = _appUserContext.ApplicationUser.Where(u => u.Id == _user.GetUserId(HttpContext.User))
                    .Include(u => u.UserCommonResource)
                    .ToList();
            //validate if user is logged in and only one user have given Id
            if (userList.Count != 1)
            {
                var error = new ErrorViewModel();
                return RedirectToAction("Index", "Error", error);
            }
            var user = userList[0];

            storage.OwnerId = user.Id;

            CommonResource common;
            //if commonResourceId specified, get it from database, add storage
            if (!string.IsNullOrEmpty(commonResourceId))
            {
                var commonList = _appUserContext.CommonResources.Where(c => c.CommonResourceId == commonResourceId)
                    .Include(c => c.Storages)
                    .ToList();
                if (commonList.Count != 1)
                {
                    var error = new ErrorViewModel();
                    return RedirectToAction("Index", "Error", error);
                }

                common = commonList[0];
                common.Storages.Add(storage);
                _appUserContext.CommonResources.Update(common);
                _appUserContext.SaveChanges();
                return RedirectToAction("Index", "Storage");
            }

            //if commonResourceId is not specified
            common = new CommonResource
            {
                CommonResourceName = commonResourceName,
                OwnerId = user.Id
            };
            common.Storages.Add(storage);
            var join = new UserCommonResource
            {
                UserCommonResourceId = null, //autogenerated by database
                UserId = user.Id,
                User = user,
                CommonResourceId = common.CommonResourceId,
                CommonResource = common
            };

            user.UserCommonResource.Add(join);

            _appUserContext.ApplicationUser.Update(user);
            await _appUserContext.SaveChangesAsync();

            return RedirectToAction("Index", "Storage");
        }





        public async Task<IActionResult> AddExistingCommon(string commonResourceId)
        {
            //get user and she's/he's CommonResources from database
            var userList = _appUserContext.ApplicationUser.Where(u => u.Id == _user.GetUserId(HttpContext.User))
                .Include(u => u.UserCommonResource)
                .ToList();
            //validate if user is logged in and only one user have given Id
            if (userList.Count != 1)
            {
                var error = new ErrorViewModel();
                return RedirectToAction("Index", "Error", error);
            }
            var user = userList[0];

            //get requested commonResources
            var commonList = _appUserContext.CommonResources.Where(c => c.CommonResourceId == commonResourceId).ToList();
            if (commonList.Count != 1)
            {
                var error = new ErrorViewModel();
                return RedirectToAction("Index", "Error", error);
            }
            var common = commonList[0];

            //add commonResource to user
            var join = new UserCommonResource
            {
                UserId = user.Id,
                User = user,
                CommonResourceId = commonResourceId,
                CommonResource = common
            };
            user.UserCommonResource.Add(join);

            //save to database
            _appUserContext.ApplicationUser.Update(user);
            await _appUserContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}