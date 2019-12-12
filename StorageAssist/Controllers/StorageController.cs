﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
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
        public async Task<IActionResult> Index()
        {
            var user = await _appUserContext.ApplicationUser.Where(u => u.Id == _user.GetUserId(HttpContext.User))
                .Include(u => u.UserCommonResource)
                    .ThenInclude(uc => uc.CommonResource)
                        .ThenInclude(c => c.Storages)
                            .ThenInclude(s => s.Products)
                .Include(u => u.UserCommonResource)
                    .ThenInclude(uc => uc.CommonResource)
                        .ThenInclude(c => c.Notes)
                .FirstOrDefaultAsync();
            if (user == null)
            {
                var error = new ErrorViewModel()
                {
                    ErrorMessage = "Error 10: unable to get user's data"
                };
                return RedirectToAction("Index", "Error", error);
            }
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
                var error = new ErrorViewModel()
                {
                    ErrorMessage = "Error 11: Storage name empty or common resource not set/empty."
                };
                return RedirectToAction("Index", "Error", error);
            }
            //get user and she's/he's CommonResources from database
            var user = await _appUserContext.ApplicationUser.Where(u => u.Id == _user.GetUserId(HttpContext.User))
                    .Include(u => u.UserCommonResource)
                    .FirstOrDefaultAsync();
            if (user == null)
            {
                var error = new ErrorViewModel()
                {
                    ErrorMessage = "Error 10: unable to get user's data"
                };
                return RedirectToAction("Index", "Error", error);
            }

            storage.OwnerId = user.Id;

            CommonResource common;
            //if commonResourceId specified, get it from database, add storage
            if (!string.IsNullOrEmpty(commonResourceId))
            {
                common = await _appUserContext.CommonResources.Where(c => c.CommonResourceId == commonResourceId)
                    .Include(c => c.Storages)
                    .FirstOrDefaultAsync();
                if (common == null)
                {
                    var error = new ErrorViewModel()
                    {
                        ErrorMessage = "Error 13"
                    };
                    return RedirectToAction("Index", "Error", error);
                }
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
            var user = await _appUserContext.ApplicationUser.Where(u => u.Id == _user.GetUserId(HttpContext.User))
                .Include(u => u.UserCommonResource)
                .FirstOrDefaultAsync();
            if (user == null)
            {
                var error = new ErrorViewModel()
                {
                    ErrorMessage = "Error 10: unable to get user's data"
                };
                return RedirectToAction("Index", "Error", error);
            }

            //get requested commonResources
            var common = await _appUserContext.CommonResources.Where(c => c.CommonResourceId == commonResourceId).FirstOrDefaultAsync();
            if (common == null)
            {
                var error = new ErrorViewModel();
                return RedirectToAction("Index", "Error", error);
            }

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
            try
            {
                _appUserContext.ApplicationUser.Update(user);
                await _appUserContext.SaveChangesAsync();
            }
            catch (InvalidOperationException)
            {
                var error = new ErrorViewModel()
                {
                    ErrorMessage = "Error 101: User already have this common resource added."
                };
                return RedirectToAction("Index", "Error", error);
            }

            return RedirectToAction("Index");
        }
    }
}