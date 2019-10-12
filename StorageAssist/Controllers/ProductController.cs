using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StorageAssist.Models;

namespace StorageAssist.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppUserContext _appUserContext;
        private readonly UserManager<IdentityUser> _user;
        public ProductController(AppUserContext appUserContext, UserManager<IdentityUser> user)
        {
            _appUserContext = appUserContext;
            _user = user;
        }
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Storage");
        }

        [HttpGet]
        public IActionResult AddProduct()
        {
            return View();
        }


        [HttpPost]
        [Authorize]
        public IActionResult AddProduct(string storageId)
        {
            System.Diagnostics.Debug.WriteLine("Debug info starts:");
            System.Diagnostics.Debug.WriteLine(storageId);
            var product = new Product()
            {
                StorageId = storageId
            };
            return View("AddProduct", product);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddProductDb([Bind("StorageId, ProductName, Type, QuantityType, Quantity, BuyDate, ExpirationDate, Comment")] Product product)
        {
            // get requested storage from db
            var storageList = _appUserContext.Storages.Where(s => s.StorageId == product.StorageId)
                .Include(s => s.Products)
                .ToList();
            // check if only one storage is in list and if it belongs to current user. If not return error
            if (storageList.Count != 1 || storageList[0].OwnerId != _user.GetUserId(HttpContext.User))
            {
                var error = new ErrorViewModel();
                return RedirectToAction("Index", "Error", error);
            }
            var storage = storageList[0];
            storage.Products.Add(product);
            product.Storage = storage;

            // update and save db
            _appUserContext.Storages.Update(storage);
            await _appUserContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DeleteProduct(string productId, string storageId)
        {
            // get requested storage from db
            var storageList = _appUserContext.Storages.Where(s => s.StorageId == storageId)
                .Include(s => s.Products)
                .ToList();
            // check if only one storage is in list and if it belongs to current user. If not return error
            if (storageList.Count != 1 || storageList[0].OwnerId != _user.GetUserId(HttpContext.User))
            {
                var error = new ErrorViewModel();
                return RedirectToAction("Index", "Error", error);
            }
            var storage = storageList[0];

            // get product
            var productList = storage.Products.Where(p => p.ProductId == productId).ToList();
            if (productList.Count != 1)
            {
                var error = new ErrorViewModel();
                return RedirectToAction("Index", "Error", error);
            }
            var product = productList[0];

            // delete product
            storage.Products.Remove(product);

            // update database
            _appUserContext.Storages.Update(storage);
            await _appUserContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }

    }
}