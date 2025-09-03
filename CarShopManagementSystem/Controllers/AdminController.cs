using System.Linq;
using System.Threading.Tasks;
using CarShopManagementSystem.Data;
using CarShopManagementSystem.Models;
using CarShopManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarShopManagementSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserRole") == "Admin";
        }

        // GET: /Admin
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin())
            {
                return Forbid();
            }

            var employees = await _context.Users.Where(u => u.Role == "Employee").ToListAsync();
            return View(employees);
        }

        // GET: /Admin/AddEmployee
        public IActionResult AddEmployee()
        {
            if (!IsAdmin())
            {
                return Forbid();
            }
            return View();
        }

        // POST: /Admin/AddEmployee
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEmployee(RegisterViewModel model)
        {
            if (!IsAdmin())
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "هذا البريد الإلكتروني مستخدم بالفعل");
                    return View(model);
                }

                var employee = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Password = model.Password, // يجب أن تكون مشفرة في تطبيق حقيقي
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    Role = "Employee"
                };

                _context.Users.Add(employee);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }
    }
}