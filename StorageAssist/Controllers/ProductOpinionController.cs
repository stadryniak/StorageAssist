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
        public async Task<ActionResult> AddProductOpinionDb([Bind("ProductName, Price, PriceOpinion, Quality, Value, Description")] ProductOpinion productOpinion)
        {
            if (!ModelState.IsValid)
            {
                ErrorViewModel error = new ErrorViewModel()
                {
                    ErrorMessage = "Invalid data"
                };
                return RedirectToAction("Index", "Error");
            }
            double priceQualityRatio = productOpinion.Quality / productOpinion.Price;
            productOpinion.PriceQualityRatio = priceQualityRatio;
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
                .Include(p=>p.ProductOpinion)
                .FirstOrDefaultAsync();
            var productOpinion = user.ProductOpinion.FirstOrDefault(po => po.ProductOpinionId == id);
            return View(productOpinion);
        }

        // POST: ProductOpinion/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind("ProductName, Price, PriceOpinion, Quality, Value, Description, ProductOpinionId")] ProductOpinion productOpinion)
        {
            try
            {
                //var user = await _dbContext.ApplicationUser.Where(u => u.Id == _user.GetUserId(User)).FirstOrDefaultAsync();
                _dbContext.ProductOpinion.Update(productOpinion);
                await _dbContext.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                var error = new ErrorViewModel()
                {
                    ErrorMessage = "Deleting product opinion failed"
                };
                return RedirectToAction("Index", "Error", error);
            }
        }

        // GET: ProductOpinion/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ProductOpinion/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}