using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StorageAssist.Models;

namespace StorageAssist.Areas.Identity.Pages.Account.Manage
{
    public class DeletePersonalDataModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<DeletePersonalDataModel> _logger;
        private readonly AppUserContext _appUserContext;

        public DeletePersonalDataModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<DeletePersonalDataModel> logger,
            AppUserContext appUserContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _appUserContext = appUserContext;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        public bool RequirePassword { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            if (RequirePassword)
            {
                if (!await _userManager.CheckPasswordAsync(user, Input.Password))
                {
                    ModelState.AddModelError(string.Empty, "Incorrect password.");
                    return Page();
                }
            }
            await DeleteDbDependencies(user);
            var result = await _userManager.DeleteAsync(user);
            var userId = await _userManager.GetUserIdAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred deleting user with ID '{userId}'.");
            }

            await _signInManager.SignOutAsync();

            _logger.LogInformation("User with ID '{UserId}' deleted themselves.", userId);

            return Redirect("~/");
        }

        private async Task DeleteDbDependencies(ApplicationUser user)
        {
            var commonList = await _appUserContext.CommonResources
                .Where(c => c.OwnerId == user.Id)
                .Include(c => c.Storages)
                    .ThenInclude(s => s.Products)
                .Include(c => c.Notes)
                .ToListAsync();


            //deletes users data from db, only if she's/he's owner
            foreach (var commonResource in commonList)
            {
                foreach (var storage in commonResource.Storages)
                {
                    _appUserContext.Products.RemoveRange(storage.Products);
                }
                _appUserContext.Storages.RemoveRange(commonResource.Storages);
                _appUserContext.RemoveRange(commonResource.Notes);
            }
            _appUserContext.RemoveRange(commonList);
            await _appUserContext.SaveChangesAsync();
        }
    }
}
