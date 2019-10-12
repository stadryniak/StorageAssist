﻿using System;
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
            //get user and she's/he's CommonResources from database
            var userList = _appUserContext.ApplicationUser.Where(u => u.Id == _user.GetUserId(HttpContext.User))
                .Include(u => u.UserCommonResource)
                .ToList();
            //validate if user is logged in and only one user have given Id
            if (userList.Count != 1)
            {
                var error = new ErrorViewModel();
                return RedirectToAction("Index", "Error", error);
            }
            var user = userList[0];

            var storageList = _appUserContext.Storages.Where(s => s.StorageId == product.StorageId)
                .Include(s => s.Products)
                .ToList();
            if (storageList.Count != 1)
            {
                var error = new ErrorViewModel();
                return RedirectToAction("Index", "Error", error);
            }
            var storage = storageList[0];
            storage.Products.Add(product);
            product.Storage = storage;

            _appUserContext.Storages.Update(storage);
            await _appUserContext.SaveChangesAsync();


            return RedirectToAction("Index");
        }

    }
}