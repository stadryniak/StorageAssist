using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        // GET: ProductOpinion/Details/5
        public async  Task<ActionResult> Details(string productId)
        {
            var user = await _dbContext.ApplicationUser
                .Where(u => u.Id == _user.GetUserId(HttpContext.User))
                .Include(u => u.ProductOpinion)
                .FirstOrDefaultAsync();
            var product = user.ProductOpinion.FirstOrDefault(p => p.ProductOpinionId == productId);
            return View(product);
        }

        // GET: ProductOpinion/Create
        public ActionResult Add()
        {
            return View();
        }

        // POST: ProductOpinion/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
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