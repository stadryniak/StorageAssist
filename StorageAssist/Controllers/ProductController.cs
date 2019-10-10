using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace StorageAssist.Controllers
{
    public class ProductController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Storage");
        }

        public IActionResult AddProduct(string storageId)
        {
            System.Diagnostics.Debug.WriteLine(storageId);
            return View();
        }
    }
}