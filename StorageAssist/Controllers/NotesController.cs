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

//TODO: edit note, multiline text, check if note is empty (and disallow adding w/ error message)

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
        public IActionResult AddNote(string Id)
        {
            var note = new Note()
            {
                CommonResourceId = Id
            };
            return View(note);
        }

        [Authorize]
        public async Task<IActionResult> AddNoteDb([Bind("NoteId, CommonResourceId, CommonResource, OwnerId, NoteName, NoteType, NoteText")] Note note, string commonResourceId, string noteName, string noteText)
        {
            var common = await _appUserContext.CommonResources.Where(c => c.CommonResourceId == commonResourceId)
                .FirstOrDefaultAsync();
            note.CommonResourceId = commonResourceId;
            note.CommonResource = common;
            note.OwnerId = _user.GetUserId(User);
            note.NoteName = noteName;
            note.NoteType = null; //unused parameter
            note.NoteText = noteText;
            common.Notes.Add(note);
            //add to database
            _appUserContext.Notes.Add(note);
            _appUserContext.CommonResources.Update(common);
            await _appUserContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }

    }
}