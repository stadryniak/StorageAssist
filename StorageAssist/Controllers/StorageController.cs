﻿using System.Linq;
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
        private readonly UserManager<ApplicationUser> _user;
        public StorageController(AppUserContext appUserContext, UserManager<ApplicationUser> user)
        {
            _appUserContext = appUserContext;
            _user = user;
        }
        // GET
        /// <summary>
        /// Get users commons, storeges, products, notes from database and returns to view
        /// </summary>
        /// <returns></returns>
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

        [HttpPost]
        [Authorize]
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

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteCommonResources(string commonResourceId)
        {
            //get user and she's/he's CommonResources from database
            var userList = _appUserContext.ApplicationUser.Where(u => u.Id == _user.GetUserId(HttpContext.User))
                .Include(u => u.UserCommonResource)
                    .ThenInclude(uc => uc.CommonResource)
                .ToList();
            //validate if user is logged in and only one user have given Id
            if (userList.Count != 1)
            {
                var error = new ErrorViewModel();
                return RedirectToAction("Index", "Error", error);
            }
            var user = userList[0];

            //get common resource
            var commonList = _appUserContext.CommonResources.Where(c => c.CommonResourceId == commonResourceId)
                .Include(c => c.UserCommonResource)
                .Include(c => c.Notes)
                .Include(c => c.Storages)
                    .ThenInclude(s => s.Products)
                .ToList();
            if (commonList.Count != 1)
            {
                var error = new ErrorViewModel();
                return RedirectToAction("Index", "Error", error);
            }
            var common = commonList[0];

            //check if user is owner of common
            if (common.OwnerId == user.Id)
            {
                common.UserCommonResource.Clear();
                //remove dependent resources
                _appUserContext.Notes.RemoveRange(common.Notes);
                _appUserContext.UserCommonResources.RemoveRange(common.UserCommonResource);
                foreach (var storage in common.Storages)
                {
                    _appUserContext.Products.RemoveRange(storage.Products);
                }
                _appUserContext.Storages.RemoveRange(common.Storages);

                _appUserContext.CommonResources.Remove(common);
                await _appUserContext.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            //remove common from user list
            var usersCommon = common.UserCommonResource.Where(uc => uc.UserId == user.Id).ToList();
            foreach (var userCommonResource in usersCommon)
            {
                _appUserContext.UserCommonResources.Remove(userCommonResource);
            }
            await _appUserContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DeleteStorage(string storageId)
        {
            //get storage
            var storageList = await _appUserContext.Storages.Where(s => s.OwnerId == _user.GetUserId(HttpContext.User)).ToListAsync();
            if (storageList.Count != 1)
            {
                var error = new ErrorViewModel();
                return RedirectToAction("Index", "Error", error);
            }
            var storage = storageList[0];

            _appUserContext.Storages.Remove(storage);
            await _appUserContext.SaveChangesAsync();

        }
    }
}