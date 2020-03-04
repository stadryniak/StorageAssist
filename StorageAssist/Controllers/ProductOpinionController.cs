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
        public async Task<ActionResult> Index()
        {
            var user = await _dbContext.ApplicationUser
                .Where(u => u.Id == _user.GetUserId(HttpContext.User))
                    .Include(u => u.ProductOpinion)
                .FirstOrDefaultAsync();
            return View(user.ProductOpinion);
        }

        public async Task<ActionResult> Details(string productId)
        {
            var user = await _dbContext.ApplicationUser
                .Where(u => u.Id == _user.GetUserId(HttpContext.User))
                .Include(u => u.ProductOpinion)
                .FirstOrDefaultAsync();
            var product = user.ProductOpinion.FirstOrDefault(p => p.ProductOpinionId == productId);
            return View(product);
        }

        [Authorize]
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
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ProductOpinion/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
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