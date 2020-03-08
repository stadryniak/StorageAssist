using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StorageAssist.Models;

namespace StorageAssist.Controllers
{
    [Authorize]
    public class ProductOpinionController : Controller
    {
        private readonly AppUserContext _dbContext;
        private readonly UserManager<ApplicationUser> _user;

        public ProductOpinionController(AppUserContext appUserContext, UserManager<ApplicationUser> user)
        {
            _dbContext = appUserContext;
            _user = user;
        }

        // GET: ProductOpinion
        [Authorize]
        public async Task<ActionResult> Index()
        {
            var user = await _dbContext.ApplicationUser
                .Where(u => u.Id == _user.GetUserId(HttpContext.User))
                .Include(u => u.ProductOpinion)
                .FirstOrDefaultAsync();
            return View(user.ProductOpinion);
        }

        [Authorize]
        public async Task<ActionResult> Details(string id)
        {
            var user = await _dbContext.ApplicationUser
                .Where(u => u.Id == _user.GetUserId(HttpContext.User))
                .Include(u => u.ProductOpinion)
                .FirstOrDefaultAsync();
            var product = user.ProductOpinion.FirstOrDefault(p => p.ProductOpinionId == id);
            return View(product);
        }

        [Authorize]
        [HttpGet]
        public ActionResult AddProductOpinion()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddProductOpinionDb(
            [Bind("ProductName, PriceOpinion, Quality, Value, Description")]
            ProductOpinion productOpinion, string price)
        {
            productOpinion.Price = double.Parse(price, System.Globalization.CultureInfo.InvariantCulture);
            if (!ModelState.IsValid)
            {
                ErrorViewModel error = new ErrorViewModel()
                {
                    ErrorMessage = "Invalid data"
                };
                return RedirectToAction("Index", "Error");
            }

            productOpinion.PriceQualityRatio = productOpinion.Quality / productOpinion.Price;
            var user = await _dbContext.ApplicationUser.Where(u => u.Id == _user.GetUserId(User)).FirstOrDefaultAsync();
            user.ProductOpinion.Add(productOpinion);
            _dbContext.Update(user);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction("Index");
        }


        // GET: ProductOpinion/Edit/5
        [HttpGet]
        public async Task<ActionResult> Edit(string id)
        {
            var user = await _dbContext.ApplicationUser.Where(u => u.Id == _user.GetUserId(User))
                .Include(p => p.ProductOpinion)
                .FirstOrDefaultAsync();
            var productOpinion = user.ProductOpinion.FirstOrDefault(po => po.ProductOpinionId == id);
            return View(productOpinion);
        }

        // POST: ProductOpinion/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(
            [Bind("ProductName, PriceOpinion, Quality, Value, Description, ProductOpinionId")]
            ProductOpinion productOpinion, string price)
        {
            productOpinion.Price = double.Parse(price, System.Globalization.CultureInfo.InvariantCulture);
            if (!ModelState.IsValid)
            {
                var error = new ErrorViewModel()
                {
                    ErrorMessage = "Invalid number"
                };
                return RedirectToAction("Index", "Error", error);
            }

            productOpinion.PriceQualityRatio = productOpinion.Quality / productOpinion.Price;
            try
            {
                _dbContext.ProductOpinion.Update(productOpinion);
                await _dbContext.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                var error = new ErrorViewModel()
                {
                    ErrorMessage = "Editing product opinion failed"
                };
                return RedirectToAction("Index", "Error", error);
            }
        }
    }
}