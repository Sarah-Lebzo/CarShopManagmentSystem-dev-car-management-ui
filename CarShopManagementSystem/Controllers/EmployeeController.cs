using System.Linq;
using System.Threading.Tasks;
using CarShopManagementSystem.Data;
using CarShopManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarShopManagementSystem.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeeController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsEmployee()
        {
            return HttpContext.Session.GetString("UserRole") == "Employee";
        }

        // GET: /Employee/Reports
        public async Task<IActionResult> Reports()
        {
            if (!IsEmployee())
            {
                return Forbid();
            }

            var transactions = await _context.Transactions
                .Include(t => t.Car)
                .Include(t => t.User)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();

            var viewModel = new EmployeeReportViewModel
            {
                TotalCarsSold = transactions.Count,
                TotalRevenue = transactions.Sum(t => t.SalePrice),
                Transactions = transactions
            };

            return View(viewModel);
        }
    }
}