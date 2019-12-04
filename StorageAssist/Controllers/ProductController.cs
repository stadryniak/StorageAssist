using System;
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
        // ReSharper disable once NotAccessedField.Local
        private readonly UserManager<ApplicationUser> _user;
        public ProductController(AppUserContext appUserContext, UserManager<ApplicationUser> user)
        {
            _appUserContext = appUserContext;
            _user = user;
        }
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Storage");
        }


        [Authorize]
        [HttpPost]
        public IActionResult AddProduct(string storageId)
        {
            var product = new Product()
            {
                StorageId = storageId
            };
            return View("AddProduct", product);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddProductDb([Bind("StorageId, ProductName, Type, QuantityType, BuyDate, ExpirationDate, Comment")] Product product, string quantity)
        {
            //add quantity to product
            product.Quantity = double.Parse(quantity, System.Globalization.CultureInfo.InvariantCulture);
            // get requested storage from db
            var storage = await _appUserContext.Storages.Where(s => s.StorageId == product.StorageId)
                .Include(s => s.Products)
                .FirstOrDefaultAsync();
            // check if only one storage is in list and if it belongs to current user. If not return error
            if (storage == null)
            {
                var error = new ErrorViewModel()
                {
                    ErrorMessage = "Error 1"
                };
                return RedirectToAction("Index", "Error", error);
            }
            storage.Products.Add(product);
            product.Storage = storage;

            // update and save db
            _appUserContext.Storages.Update(storage);
            await _appUserContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> EditProduct(string id)
        {
            //get product from db
            var product = await _appUserContext.Products.FirstOrDefaultAsync(p => p.ProductId == id);
            if (product == null)
            {
                var error = new ErrorViewModel()
                {
                    ErrorMessage = "Invalid product"
                };
                return RedirectToAction("Index", "Error", error);
            }

            return View(product);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> EditProductDb([Bind("ProductId, StorageId, ProductName, Type, QuantityType, BuyDate, ExpirationDate, Comment")] Product product, string quantity)
        {
            product.Quantity = double.Parse(quantity, System.Globalization.CultureInfo.InvariantCulture);
            _appUserContext.Entry(product).State = EntityState.Modified;
            await _appUserContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}