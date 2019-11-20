using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StorageAssist.Models;

namespace StorageAssist.Controllers
{
    public class NotesController : Controller
    {
        private readonly AppUserContext _appUserContext;
        private readonly UserManager<ApplicationUser> _user;
        public NotesController(AppUserContext appUserContext, UserManager<ApplicationUser> user)
        {
            _appUserContext = appUserContext;
            _user = user;
        }
        [Authorize]
        public IActionResult Index()
        {
            var common = _appUserContext.CommonResources.Where(c => c.OwnerId == _user.GetUserId(User))
                .Include(c => c.Notes);
            if (common == null)
            {
                var error = new ErrorViewModel()
                {
                    ErrorMessage = "Error 173: unable to get user's common resources"
                };
                return RedirectToAction("Index", "Error", error);
            }
            return View(common);
        }
    }
}