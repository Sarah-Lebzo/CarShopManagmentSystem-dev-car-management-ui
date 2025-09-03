using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarShopManagementSystem.Data;
using CarShopManagementSystem.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace CarShopManagementSystem.Controllers
{
    public class CarsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public CarsController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        public async Task<IActionResult> BookingConfirmation(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Car)
                    .ThenInclude(c => c.Manufacturer)   // جلب المصنع
                .Include(b => b.Car)
                    .ThenInclude(c => c.SubBrand)       // جلب السب براند
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        [HttpPost, ActionName("Book")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookConfirmed(int id)
        {
            // Check if user is logged in
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Fetch car with Manufacturer and SubBrand
            var car = await _context.Cars
                .Include(c => c.Manufacturer)
                .Include(c => c.SubBrand)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsAvailable);

            if (car == null)
            {
                return NotFound();
            }

            // Get user
            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
            {
                return NotFound();
            }

            // Create booking
            var booking = new Booking
            {
                UserId = userId.Value,
                CarId = id,
                BookingDate = DateTime.Now,
                ExpiryDate = DateTime.Now.AddHours(24),
                IsActive = true,
                User = user,
                Car = car
            };

            // Update car availability
            car.IsAvailable = false;

            _context.Bookings.Add(booking);
            _context.Update(car);
            await _context.SaveChangesAsync();

            // Redirect to booking confirmation page
            return RedirectToAction(nameof(BookingConfirmation), new { id = booking.Id });
        }

        public async Task<IActionResult> MyBookings()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var bookings = await _context.Bookings
                .Include(b => b.Car)
                .Where(b => b.UserId == userId && b.IsActive)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            return View(bookings);
        }

        // GET: CancelBooking
        public async Task<IActionResult> CancelBooking(int? id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Car)
                    .ThenInclude(c => c.Manufacturer) // جلب المصنع
                .Include(b => b.Car)
                    .ThenInclude(c => c.SubBrand)     // جلب السب براند
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId && m.IsActive);

            if (booking == null) return NotFound();

            return View(booking);
        }

        // POST: CancelBookingConfirmed
        [HttpPost, ActionName("CancelBooking")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelBookingConfirmed(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var booking = await _context.Bookings
                .Include(b => b.Car)
                    .ThenInclude(c => c.Manufacturer) // جلب المصنع
                .Include(b => b.Car)
                    .ThenInclude(c => c.SubBrand)     // جلب السب براند
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId && m.IsActive);

            if (booking == null) return NotFound();

            booking.IsActive = false;
            booking.Car.IsAvailable = true;

            _context.Update(booking);
            _context.Update(booking.Car);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyBookings));
        }

        public async Task<IActionResult> Index(string searchString, bool? isUsed)
        {
            var cars = _context.Cars.Include(c => c.Manufacturer).AsQueryable();

            DateTime now = DateTime.Now;
            cars = cars.Where(c => c.IsAvailable || c.SoldDate == null || c.SoldDate.Value.AddMinutes(10) > now);

            if (!string.IsNullOrEmpty(searchString))
            {
                cars = cars.Where(c =>
                    c.Manufacturer.Name.Contains(searchString) ||
                    c.Model.Contains(searchString) ||
                    c.Description.Contains(searchString));
            }

            if (isUsed.HasValue)
            {
                cars = cars.Where(c => c.IsUsed == isUsed.Value);
            }

            return View(await cars.OrderByDescending(c => c.DateAdded).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var car = await _context.Cars
                .Include(c => c.Manufacturer)   // جلب المصنع
                .Include(c => c.SubBrand)       // جلب السب براند
                .FirstOrDefaultAsync(m => m.Id == id);

            if (car == null) return NotFound();

            return View(car);
        }


        public IActionResult Create()
        {
            // التحقق من صلاحية الـ Admin
            if (HttpContext.Session.GetInt32("IsAdmin") != 1)
                return RedirectToAction("Index", "Home");

            ViewData["Brands"] = _context.Manufacturers.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Car car, IFormFile ImageFile)
        {
            if (HttpContext.Session.GetInt32("IsAdmin") != 1)
                return RedirectToAction("Index", "Cars");

            if (car == null)
                return BadRequest("سيارة فارغة");

            if (ModelState.IsValid)
            {
                try
                {
                    // رفع الصورة
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "cars");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ImageFile.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(fileStream);
                        }

                        car.ImageUrl = "/images/cars/" + uniqueFileName;
                    }
                    else
                    {
                        car.ImageUrl = "/images/cars/default-car.jpg";
                    }

                    car.DateAdded = DateTime.Now;
                    car.IsAvailable = true;

                    _context.Add(car);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "تمت إضافة السيارة بنجاح!";
                    return RedirectToAction(nameof(Create));
                }
                catch (Exception ex)
                {
                    // عرض رسالة الخطأ مع التفاصيل الداخلية إذا وجدت
                    TempData["ErrorMessage"] = "حدث خطأ أثناء الحفظ: " + (ex.InnerException?.Message ?? ex.Message);
                    ViewData["Brands"] = _context.Manufacturers.ToList();
                    return View(car);
                }
            }

            // إذا لم يكن النموذج صالح
            TempData["ErrorMessage"] = "الرجاء التأكد من تعبئة جميع الحقول المطلوبة بشكل صحيح.";
            ViewData["Brands"] = _context.Manufacturers.ToList();
            return View(car);
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (HttpContext.Session.GetInt32("IsAdmin") != 1)
                return RedirectToAction("Index", "Home");
            if (id == null) return NotFound();

            var car = await _context.Cars
                .Include(c => c.Manufacturer)
                .Include(c => c.SubBrand)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (car == null) return NotFound();

            ViewData["Brands"] = await _context.Manufacturers.ToListAsync();
            return View(car);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Car car, IFormFile ImageFile)
        {
            if (HttpContext.Session.GetInt32("IsAdmin") != 1)
                return RedirectToAction("Index", "Home");
            if (id != car.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "cars");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        // حذف الصورة القديمة إذا موجودة وغير الصورة الافتراضية
                        if (!string.IsNullOrEmpty(car.ImageUrl) && !car.ImageUrl.Contains("default-car.jpg"))
                        {
                            string oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, car.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                                System.IO.File.Delete(oldImagePath);
                        }

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + ImageFile.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(fileStream);
                        }

                        car.ImageUrl = "/images/cars/" + uniqueFileName;
                    }

                    // تحديث السيارة في قاعدة البيانات
                    _context.Update(car);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Cars.AnyAsync(e => e.Id == car.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            // إعادة تحميل الـ View مع البيانات المطلوبة للـ Dropdowns
            ViewData["Brands"] = await _context.Manufacturers.ToListAsync();
            return View(car);
        }


        // GET: Cars/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            // التأكد من صلاحية المسؤول
            if (HttpContext.Session.GetInt32("IsAdmin") != 1)
                return RedirectToAction("Index", "Home");

            if (id == null) return NotFound();

            // جلب السيارة مع المصنع والسب براند
            var car = await _context.Cars
                .Include(c => c.Manufacturer)
                .Include(c => c.SubBrand)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (car == null) return NotFound();

            return View(car);
        }

        // POST: Cars/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // التأكد من صلاحية المسؤول
            if (HttpContext.Session.GetInt32("IsAdmin") != 1)
                return RedirectToAction("Index", "Home");

            var car = await _context.Cars
                .Include(c => c.Manufacturer)
                .Include(c => c.SubBrand)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (car != null)
            {
                // حذف صورة السيارة إذا لم تكن الصورة الافتراضية
                if (!string.IsNullOrEmpty(car.ImageUrl) && !car.ImageUrl.Contains("default-car.jpg"))
                {
                    string imagePath = Path.Combine(_hostEnvironment.WebRootPath, car.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                        System.IO.File.Delete(imagePath);
                }

                _context.Cars.Remove(car);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        //GET: Cars/Book/5
        public async Task<IActionResult> Book(int? id)
        {
            // تحقق من تسجيل الدخول
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Book", "Cars", new { id }) });
            }

            if (id == null)
            {
                return NotFound();
            }

            // جلب السيارة مع Manufacturer و SubBrand
            var car = await _context.Cars
                .Include(c => c.Manufacturer)
                .Include(c => c.SubBrand)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsAvailable);

            if (car == null)
            {
                TempData["ErrorMessage"] = "هذه السيارة غير متاحة للحجز الآن.";
                return RedirectToAction(nameof(Index));
            }

            return View(car);
        }


        // إرجاع SubBrands بناءً على BrandId
        [HttpGet]
        public IActionResult GetSubBrands(int manufacturerId)
        {
            var subBrands = _context.SubBrands
                .Where(s => s.ManufacturerId == manufacturerId)
                .Select(s => new
                {
                    Id = s.Id,
                    Name = s.Name
                })
                .ToList();

            return Json(subBrands);
        }

        // POST: Cars/CheckoutConfirmed/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckoutConfirmed(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var booking = await _context.Bookings
                .Include(b => b.Car)
                .FirstOrDefaultAsync(b => b.Id == id && b.IsActive);

            if (booking == null || booking.UserId != userId)
            {
                return NotFound();
            }

            // Redirect to payment page
            return RedirectToAction("Index", "Payment", new { bookingId = booking.Id });
        }

    }
}



//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore;
//using CarShopManagementSystem.Data;
//using CarShopManagementSystem.Models;
//using Microsoft.AspNetCore.Http;
//using System.IO;
//using Microsoft.AspNetCore.Hosting;

//namespace CarShopManagementSystem.Controllers
//{
//    public class CarsController : Controller
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly IWebHostEnvironment _hostEnvironment;

//        public CarsController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
//        {
//            _context = context;
//            _hostEnvironment = hostEnvironment;
//        }

//        // GET: Cars/BookingConfirmation/5
//        public async Task<IActionResult> BookingConfirmation(int? id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            var booking = await _context.Bookings
//                .Include(b => b.Car)
//                .Include(b => b.User)
//                .FirstOrDefaultAsync(m => m.Id == id);

//            if (booking == null)
//            {
//                return NotFound();
//            }

//            return View(booking);
//        }

//        // GET: Cars/MyBookings
//        public async Task<IActionResult> MyBookings()
//        {
//            // Check if user is logged in
//            int? userId = HttpContext.Session.GetInt32("UserId");
//            if (userId == null)
//            {
//                return RedirectToAction("Login", "Account");
//            }

//            var bookings = await _context.Bookings
//                .Include(b => b.Car)
//                .Where(b => b.UserId == userId && b.IsActive)
//                .OrderByDescending(b => b.BookingDate)
//                .ToListAsync();

//            return View(bookings);
//        }

//        // GET: Cars/CancelBooking/5
//        public async Task<IActionResult> CancelBooking(int? id)
//        {
//            // Check if user is logged in
//            int? userId = HttpContext.Session.GetInt32("UserId");
//            if (userId == null)
//            {
//                return RedirectToAction("Login", "Account");
//            }

//            if (id == null)
//            {
//                return NotFound();
//            }

//            var booking = await _context.Bookings
//                .Include(b => b.Car)
//                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId && m.IsActive);

//            if (booking == null)
//            {
//                return NotFound();
//            }

//            return View(booking);
//        }

//        // POST: Cars/CancelBooking/5
//        [HttpPost, ActionName("CancelBooking")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> CancelBookingConfirmed(int id)
//        {
//            // Check if user is logged in
//            int? userId = HttpContext.Session.GetInt32("UserId");
//            if (userId == null)
//            {
//                return RedirectToAction("Login", "Account");
//            }

//            var booking = await _context.Bookings
//                .Include(b => b.Car)
//                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId && m.IsActive);

//            if (booking == null)
//            {
//                return NotFound();
//            }

//            // Update booking status
//            booking.IsActive = false;

//            // Make car available again
//            booking.Car.IsAvailable = true;

//            _context.Update(booking);
//            _context.Update(booking.Car);
//            await _context.SaveChangesAsync();

//            return RedirectToAction(nameof(MyBookings));
//        }

//        // GET: Cars
//        public async Task<IActionResult> Index(string searchString, string sortOrder, bool? isUsed)
//        {
//            ViewData["CurrentFilter"] = searchString;
//            ViewData["CurrentSort"] = sortOrder;
//            ViewData["IsUsedFilter"] = isUsed;

//            ViewData["MakeSortParam"] = sortOrder == "make_asc" ? "make_desc" : "make_asc";
//            ViewData["ModelSortParam"] = sortOrder == "model_asc" ? "model_desc" : "model_asc";
//            ViewData["YearSortParam"] = sortOrder == "year_asc" ? "year_desc" : "year_asc";
//            ViewData["PriceSortParam"] = sortOrder == "price_asc" ? "price_desc" : "price_asc";

//            var cars = _context.Cars.AsQueryable();

//            // إخفاء السيارات المباعة بعد 10 دقائق من وقت البيع
//            DateTime now = DateTime.Now;
//            cars = cars.Where(c => c.IsAvailable || c.SoldDate == null || c.SoldDate.Value.AddMinutes(10) > now);

//            // Apply filters
//            if (!string.IsNullOrEmpty(searchString))
//            {
//                cars = cars.Where(c => c.Make.Contains(searchString) || 
//                                     c.Model.Contains(searchString) || 
//                                     c.Description.Contains(searchString));
//            }

//            if (isUsed.HasValue)
//            {
//                cars = cars.Where(c => c.IsUsed == isUsed.Value);
//            }

//            // Apply sorting
//            switch (sortOrder)
//            {
//                case "make_asc":
//                    cars = cars.OrderBy(c => c.Make);
//                    break;
//                case "make_desc":
//                    cars = cars.OrderByDescending(c => c.Make);
//                    break;
//                case "model_asc":
//                    cars = cars.OrderBy(c => c.Model);
//                    break;
//                case "model_desc":
//                    cars = cars.OrderByDescending(c => c.Model);
//                    break;
//                case "year_asc":
//                    cars = cars.OrderBy(c => c.Year);
//                    break;
//                case "year_desc":
//                    cars = cars.OrderByDescending(c => c.Year);
//                    break;
//                case "price_asc":
//                    cars = cars.OrderBy(c => c.Price);
//                    break;
//                case "price_desc":
//                    cars = cars.OrderByDescending(c => c.Price);
//                    break;
//                default:
//                    cars = cars.OrderByDescending(c => c.DateAdded);
//                    break;
//            }

//            return View(await cars.ToListAsync());
//        }

//        // GET: Cars/Details/5
//        public async Task<IActionResult> Details(int? id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            var car = await _context.Cars
//                .FirstOrDefaultAsync(m => m.Id == id);
//            if (car == null)
//            {
//                return NotFound();
//            }

//            return View(car);
//        }

//        // GET: Cars/Create
//        public IActionResult Create()
//        {
//            // Check if user is admin
//            if (HttpContext.Session.GetInt32("IsAdmin") != 1)
//            {
//                return RedirectToAction("Index", "Home");
//            }

//            return View();
//        }

//        // POST: Cars/Create
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create([Bind("Id,Make,Model,Year,Color,Price,Description,IsUsed,Mileage,ImageUrl,IsAvailable")] Car car, IFormFile imageFile)
//        {
//            // Check if user is admin
//            if (HttpContext.Session.GetInt32("IsAdmin") != 1)
//            {
//                return RedirectToAction("Index", "Home");
//            }

//            if (ModelState.IsValid)
//            {
//                // Handle image upload
//                if (imageFile != null && imageFile.Length > 0)
//                {
//                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "cars");
//                    if (!Directory.Exists(uploadsFolder))
//                    {
//                        Directory.CreateDirectory(uploadsFolder);
//                    }

//                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
//                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

//                    using (var fileStream = new FileStream(filePath, FileMode.Create))
//                    {
//                        await imageFile.CopyToAsync(fileStream);
//                    }

//                    car.ImageUrl = "/images/cars/" + uniqueFileName;
//                }
//                else
//                {
//                    // Default image if no image is uploaded
//                    car.ImageUrl = "/images/cars/default-car.jpg";
//                }

//                car.DateAdded = DateTime.Now;
//                _context.Add(car);
//                await _context.SaveChangesAsync();
//                return RedirectToAction(nameof(Index));
//            }
//            return View(car);
//        }

//        // GET: Cars/Edit/5
//        public async Task<IActionResult> Edit(int? id)
//        {
//            // Check if user is admin
//            if (HttpContext.Session.GetInt32("IsAdmin") != 1)
//            {
//                return RedirectToAction("Index", "Home");
//            }

//            if (id == null)
//            {
//                return NotFound();
//            }

//            var car = await _context.Cars.FindAsync(id);
//            if (car == null)
//            {
//                return NotFound();
//            }
//            return View(car);
//        }

//        // POST: Cars/Edit/5
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(int id, [Bind("Id,Make,Model,Year,Color,Price,Description,IsUsed,Mileage,ImageUrl,IsAvailable,DateAdded")] Car car, IFormFile imageFile)
//        {
//            // Check if user is admin
//            if (HttpContext.Session.GetInt32("IsAdmin") != 1)
//            {
//                return RedirectToAction("Index", "Home");
//            }

//            if (id != car.Id)
//            {
//                return NotFound();
//            }

//            if (ModelState.IsValid)
//            {
//                try
//                {
//                    // Handle image upload
//                    if (imageFile != null && imageFile.Length > 0)
//                    {
//                        string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "cars");
//                        if (!Directory.Exists(uploadsFolder))
//                        {
//                            Directory.CreateDirectory(uploadsFolder);
//                        }

//                        // Delete old image if it exists and is not the default image
//                        if (!string.IsNullOrEmpty(car.ImageUrl) && !car.ImageUrl.Contains("default-car.jpg"))
//                        {
//                            string oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, car.ImageUrl.TrimStart('/'));
//                            if (System.IO.File.Exists(oldImagePath))
//                            {
//                                System.IO.File.Delete(oldImagePath);
//                            }
//                        }

//                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
//                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

//                        using (var fileStream = new FileStream(filePath, FileMode.Create))
//                        {
//                            await imageFile.CopyToAsync(fileStream);
//                        }

//                        car.ImageUrl = "/images/cars/" + uniqueFileName;
//                    }

//                    _context.Update(car);
//                    await _context.SaveChangesAsync();
//                }
//                catch (DbUpdateConcurrencyException)
//                {
//                    if (!CarExists(car.Id))
//                    {
//                        return NotFound();
//                    }
//                    else
//                    {
//                        throw;
//                    }
//                }
//                return RedirectToAction(nameof(Index));
//            }
//            return View(car);
//        }

//        // GET: Cars/Delete/5
//        public async Task<IActionResult> Delete(int? id)
//        {
//            // Check if user is admin
//            if (HttpContext.Session.GetInt32("IsAdmin") != 1)
//            {
//                return RedirectToAction("Index", "Home");
//            }

//            if (id == null)
//            {
//                return NotFound();
//            }

//            var car = await _context.Cars
//                .FirstOrDefaultAsync(m => m.Id == id);
//            if (car == null)
//            {
//                return NotFound();
//            }

//            return View(car);
//        }

//        // POST: Cars/Delete/5
//        [HttpPost, ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeleteConfirmed(int id)
//        {
//            // Check if user is admin
//            if (HttpContext.Session.GetInt32("IsAdmin") != 1)
//            {
//                return RedirectToAction("Index", "Home");
//            }

//            var car = await _context.Cars.FindAsync(id);

//            // Delete image if it exists and is not the default image
//            if (car != null && !string.IsNullOrEmpty(car.ImageUrl) && !car.ImageUrl.Contains("default-car.jpg"))
//            {
//                string imagePath = Path.Combine(_hostEnvironment.WebRootPath, car.ImageUrl.TrimStart('/'));
//                if (System.IO.File.Exists(imagePath))
//                {
//                    System.IO.File.Delete(imagePath);
//                }
//            }

//            _context.Cars.Remove(car);
//            await _context.SaveChangesAsync();
//            return RedirectToAction(nameof(Index));
//        }

//        private bool CarExists(int id)
//        {
//            return _context.Cars.Any(e => e.Id == id);
//        }

//        // GET: Cars/Book/5
//        public async Task<IActionResult> Book(int? id)
//        {
//            // Check if user is logged in
//            int? userId = HttpContext.Session.GetInt32("UserId");
//            if (userId == null)
//            {
//                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Book", "Cars", new { id }) });
//            }

//            if (id == null)
//            {
//                return NotFound();
//            }

//            var car = await _context.Cars
//                .FirstOrDefaultAsync(m => m.Id == id && m.IsAvailable);
//            if (car == null)
//            {
//                return NotFound();
//            }

//            return View(car);
//        }

//        // POST: Cars/Book/5
//        [HttpPost, ActionName("Book")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> BookConfirmed(int id)
//        {
//            // Check if user is logged in
//            int? userId = HttpContext.Session.GetInt32("UserId");
//            if (userId == null)
//            {
//                return RedirectToAction("Login", "Account");
//            }

//            var car = await _context.Cars.FindAsync(id);
//            if (car == null || !car.IsAvailable)
//            {
//                return NotFound();
//            }

//            // Get user
//            var user = await _context.Users.FindAsync(userId.Value);

//            if (user == null)
//            {
//                return NotFound();
//            }

//            // Create booking
//            var booking = new Booking
//            {
//                UserId = userId.Value,
//                CarId = id,
//                BookingDate = DateTime.Now,
//                ExpiryDate = DateTime.Now.AddHours(24),
//                IsActive = true,
//                User = user,
//                Car = car
//            };

//            // Update car availability
//            car.IsAvailable = false;

//            _context.Bookings.Add(booking);
//            _context.Update(car);
//            await _context.SaveChangesAsync();

//            // Redirect to booking confirmation page
//            return RedirectToAction(nameof(BookingConfirmation), new { id = booking.Id });
//        }
//        // POST: Cars/CheckoutConfirmed/5
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> CheckoutConfirmed(int id)
//        {
//            int? userId = HttpContext.Session.GetInt32("UserId");
//            if (userId == null)
//            {
//                return RedirectToAction("Login", "Account");
//            }

//            var booking = await _context.Bookings
//                .Include(b => b.Car)
//                .FirstOrDefaultAsync(b => b.Id == id && b.IsActive);

//            if (booking == null || booking.UserId != userId)
//            {
//                return NotFound();
//            }

//            // Redirect to payment page
//            return RedirectToAction("Index", "Payment", new { bookingId = booking.Id });
//        }

//    }
//}