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
        public async Task<IActionResult> Index(string id, string typePost, string ownerId)
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
                    var usersCommon = ((CommonResource) toDelete).UserCommonResource.Where(uc => uc.UserId == _user.GetUserId(User)).ToList();
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
                default: break;
            }
            Delete(toDelete);
            return View();
        }


        /// <summary>
        /// This generic class handles deletion of every object in database.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toDeletion">Object to delete from database</param>
        private void Delete<T>(T toDeletion)
        {
            System.Diagnostics.Debug.WriteLine(toDeletion);
            _user.GetUserId(User);
        }
    }
}