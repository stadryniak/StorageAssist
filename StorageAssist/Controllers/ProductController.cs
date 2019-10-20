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

        [HttpGet]
        public IActionResult AddProduct()
        {
            return View();
        }


        [HttpPost]
        [Authorize]
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
            var storageList = _appUserContext.Storages.Where(s => s.StorageId == product.StorageId)
                .Include(s => s.Products)
                .ToList();
            // check if only one storage is in list and if it belongs to current user. If not return error
            if (storageList.Count != 1)
            {
                var error = new ErrorViewModel()
                {
                    ErrorMessage = "Error 1"
                };
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
            if (storageList.Count != 1)
            {
                var error = new ErrorViewModel()
                {
                    ErrorMessage = "Error 2"
                };
                return RedirectToAction("Index", "Error", error);
            }
            var storage = storageList[0];

            // get product
            var productList = storage.Products.Where(p => p.ProductId == productId).ToList();
            if (productList.Count != 1)
            {
                var error = new ErrorViewModel()
                {
                    ErrorMessage = "Error 3"
                };
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

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> EditProduct(string productId)
        {
            //get product from db
            var product = await _appUserContext.Products.FirstOrDefaultAsync(p => p.ProductId == productId);

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