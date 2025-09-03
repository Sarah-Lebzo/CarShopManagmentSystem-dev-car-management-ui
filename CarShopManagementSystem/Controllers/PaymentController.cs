using System;
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
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Payment
        public async Task<IActionResult> Index(int bookingId, string message = null)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            string role = HttpContext.Session.GetString("UserRole");
            if (userId == null || role != "Customer")
            {
                return Forbid();
            }

            var booking = await _context.Bookings
                .Include(b => b.Car)
                    .ThenInclude(c => c.Manufacturer)  // جلب الشركة
                .Include(b => b.Car)
                    .ThenInclude(c => c.SubBrand)     // جلب السب براند
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.IsActive && b.UserId == userId);

            if (booking == null)
            {
                return NotFound();
            }

            var viewModel = new PaymentViewModel
            {
                Booking = booking,
                Message = message,
                ShippingCountry = null,
                ShippingMethod = null
            };

            return View(viewModel);
        }


        // POST: Payment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(int bookingId, string cardNumber, int expiryMonth, int expiryYear, string cvv, string shippingCountry, string shippingMethod)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            string role = HttpContext.Session.GetString("UserRole");
            if (userId == null || role != "Customer")
            {
                return Forbid();
            }

            var booking = await _context.Bookings
                .Include(b => b.Car)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.IsActive && b.UserId == userId);

            if (booking == null)
            {
                return NotFound();
            }

            var card = await _context.CreditCards.FirstOrDefaultAsync(c => c.CardNumber == cardNumber && c.ExpiryMonth == expiryMonth && c.ExpiryYear == expiryYear && c.CVV == cvv);

            if (card == null || !card.IsValid || card.ExpiryYear < DateTime.Now.Year || (card.ExpiryYear == DateTime.Now.Year && card.ExpiryMonth < DateTime.Now.Month) || card.Balance < booking.Car.EstimatedPriceMax)
            {
                string errorMsg = "عملية الشراء مرفوضة: البطاقة غير صالحة أو الرصيد غير كافٍ.";
                return await Index(bookingId, errorMsg);
            }

            // Debit amount
            card.Balance -= booking.Car.EstimatedPriceMax;

            // Create transaction
            var transaction = new Transaction
            {
                UserId = userId.Value,
                CarId = booking.CarId,
                SalePrice = booking.Car.EstimatedPriceMax,
                TransactionDate = DateTime.Now,
                Notes = $"Purchase paid with card {card.CardNumber.Substring(card.CardNumber.Length - 4)}"
            };

            // Update booking
            booking.IsActive = false;

            // Mark car as sold
            var car = booking.Car;
            car.IsAvailable = false;
            car.SoldDate = DateTime.Now;

            _context.Transactions.Add(transaction);
            _context.Update(card);
            _context.Update(booking);
            _context.Update(car);
            await _context.SaveChangesAsync();

            string successMsg = $"تمت عملية الشراء بنجاح! سيتم شحن السيارة إلى {shippingCountry} عبر {shippingMethod}. ستصلك خلال 7-14 يومًا.";
            return RedirectToAction("Confirmation", new { transactionId = transaction.Id, message = successMsg });
        }

        public async Task<IActionResult> Confirmation(int transactionId, string message)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Car)
                    .ThenInclude(c => c.Manufacturer)
                .Include(t => t.Car)
                    .ThenInclude(c => c.SubBrand)
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == transactionId);

            if (transaction == null)
            {
                return NotFound();
            }

            ViewBag.Message = message;
            return View(transaction);
        }

    }
}