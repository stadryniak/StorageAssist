using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StorageAssist.Models;

namespace StorageAssist.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult Index(ErrorViewModel error)
        {
            return View("Error", error);
        }
    }
}