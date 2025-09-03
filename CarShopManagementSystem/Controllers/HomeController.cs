using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CarShopManagementSystem.Models;
using CarShopManagementSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace CarShopManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var featuredCars = await _context.Cars
                .Include(c => c.Manufacturer)
                .Include(c => c.SubBrand)
                .OrderByDescending(c => c.DateAdded)
                .Take(6)
                .ToListAsync();

            return View(featuredCars);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}



//using System.Diagnostics;
//using Microsoft.AspNetCore.Mvc;
//using CarShopManagementSystem.Models;
//using CarShopManagementSystem.Data;
//using Microsoft.EntityFrameworkCore;

//namespace CarShopManagementSystem.Controllers;

//public class HomeController : Controller
//{
//    private readonly ILogger<HomeController> _logger;
//    private readonly ApplicationDbContext _context;

//    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
//    {
//        _logger = logger;
//        _context = context;
//    }

//    public async Task<IActionResult> Index()
//    {
//        // Get the latest cars for the featured section
//        var featuredCars = await _context.Cars
//            .OrderByDescending(c => c.DateAdded)
//            .Take(6)
//            .ToListAsync();

//        return View(featuredCars);
//    }

//    public IActionResult Privacy()
//    {
//        return View();
//    }

//    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
//    public IActionResult Error()
//    {
//        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
//    }
//}
