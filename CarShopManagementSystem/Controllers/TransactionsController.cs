using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using CarShopManagementSystem.Data;
using CarShopManagementSystem.Models;

namespace CarShopManagementSystem.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransactionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Transactions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(t => t.Car)
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction == null)
            {
                return NotFound();
            }

            // Ensure that only the owner or admin can view the transaction
            int? userId = HttpContext.Session.GetInt32("UserId");
            bool isAdmin = HttpContext.Session.GetInt32("IsAdmin") == 1;
            if (!isAdmin && (userId == null || transaction.UserId != userId))
            {
                return Forbid();
            }

            return View(transaction);
        }
    }
}