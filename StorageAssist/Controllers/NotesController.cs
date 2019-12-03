using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
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
        public async Task<IActionResult> Index()
        {
            var userCommons = await _appUserContext.UserCommonResources.Where(uc => uc.UserId == _user.GetUserId(User))
                .Include(uc => uc.CommonResource)
                    .ThenInclude(c => c.Notes).ToListAsync();
            if (userCommons.Count == 0)
            {
                var error = new ErrorViewModel()
                {
                    ErrorMessage = "Error 173: unable to get user's common resources. Possibly user don't have resources."
                };
                return RedirectToAction("Index", "Error", error);
            }

            var commons = userCommons.Select(common => common.CommonResource).ToList();

            return View(commons);
        }

        [Authorize]
        public async Task<IActionResult> AddNoteDb(string commonId, string noteName, string text)
        {
            var common = await _appUserContext.CommonResources.Where(c => c.CommonResourceId == commonId)
                .FirstOrDefaultAsync();
            Note note = new Note
            {
                CommonResourceId = commonId,
                CommonResource = common,
                OwnerId = _user.GetUserId(User),
                NoteName = noteName,
                NoteType = null,
                NoteText = text
            };
            common.Notes.Add(note);
            _appUserContext.Notes.Add(note);
            _appUserContext.CommonResources.Update(common);
            await _appUserContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }

    }
}