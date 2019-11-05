using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using StorageAssist.Models;

namespace StorageAssist.Controllers
{
    public class DeleteController : Controller
    {
        private readonly AppUserContext _appUserContext;
        private readonly UserManager<ApplicationUser> _user;
        public DeleteController(AppUserContext appUserContext, UserManager<ApplicationUser> user)
        {
            _appUserContext = appUserContext;
            _user = user;
        }
        //gets id, type and owner
        public IActionResult Index(string id, string typePost, string name, string ownerId)
        {
            ViewBag.id = id;
            ViewBag.typePost = typePost;
            ViewBag.name = name;
            ViewBag.isOwner = ownerId == _user.GetUserId(User);
            return View();
        }

        public async Task<IActionResult> DeleteTask(string id, string typePost)
        {
            var type = "StorageAssist.Models." + typePost;
            Type t = Type.GetType(type);
            if (t == null)
            {
                var error = new ErrorViewModel()
                {
                    ErrorMessage = "Error 31"
                };
                return RedirectToAction("Index", "Error", error);
            }

            var toDelete = Activator.CreateInstance(t);
            switch (toDelete)
            {
                case CommonResource common:
                    toDelete = await _appUserContext.CommonResources
                        .Where(c => c.CommonResourceId == id)
                        .Include(c => c.Storages)
                        .ThenInclude(s => s.Products)
                        .Include(c => c.Notes)
                        .Include(c => c.UserCommonResource)
                        .FirstOrDefaultAsync();
                    break;
                case Storage storage:
                    toDelete = await _appUserContext.Storages
                        .Where(s => s.StorageId == id)
                        .Include(s => s.Products)
                        .FirstOrDefaultAsync();
                    break;
                case Product product:
                    toDelete = await _appUserContext.Products
                        .Where(p => p.ProductId == id)
                        .FirstOrDefaultAsync();
                    break;
                case Note note:
                    toDelete = await _appUserContext.Notes
                        .Where(n => n.NoteId == id)
                        .FirstOrDefaultAsync();
                    break;
                default:
                    var error = new ErrorViewModel()
                    {
                        ErrorMessage = "Error 60"
                    };
                    return RedirectToAction("Index", "Error", error);
            }
            await Delete(toDelete);
            return RedirectToAction("Index", "Storage");
        }

        /// <summary>
        /// This generic class handles deletion of every object in database.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toDelete">Object to delete from database</param>
        private async Task Delete<T>(T toDelete)
        {
            switch (toDelete)
            {
                case CommonResource common:
                    if (common.OwnerId == _user.GetUserId(User))
                    {

                        //remove dependent resources
                        _appUserContext.Notes.RemoveRange(common.Notes);
                        _appUserContext.UserCommonResources.RemoveRange(common.UserCommonResource);
                        foreach (var storage in common.Storages)
                        {
                            _appUserContext.Products.RemoveRange(storage.Products);
                        }

                        _appUserContext.Storages.RemoveRange(common.Storages);

                        _appUserContext.CommonResources.Remove(common);
                        break;
                    }
                    //remove common from user list
                    var usersCommon = common.UserCommonResource.Where(uc => uc.UserId == _user.GetUserId(User)).ToList();
                    foreach (var userCommonResource in usersCommon)
                    {
                        _appUserContext.UserCommonResources.Remove(userCommonResource);
                    }
                    break;
                case Storage storage:
                    _appUserContext.Products.RemoveRange(storage.Products);
                    _appUserContext.Storages.Remove(storage);
                    break;
                case Product product:
                    _appUserContext.Products.Remove(product);
                    break;
                case Note note:
                    _appUserContext.Notes.Remove(note);
                    break;
            }
            await _appUserContext.SaveChangesAsync();
        }
    }
}