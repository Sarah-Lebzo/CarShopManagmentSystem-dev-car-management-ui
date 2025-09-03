using CarShopManagementSystem.Data;
using CarShopManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarShopManagementSystem.Controllers
{
    public class ManufacturersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ManufacturersController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Manufacturers
        public async Task<IActionResult> Index()
        {
            var manufacturers = await _context.Manufacturers
                .Include(m => m.SubBrands)
                .ToListAsync();

            return View(manufacturers);
        }

        // GET: Manufacturers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var manufacturer = await _context.Manufacturers
                .Include(m => m.SubBrands)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (manufacturer == null) return NotFound();

            return View(manufacturer);
        }

        // GET: Manufacturers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Manufacturers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string Name, IFormFile? LogoFile, List<string> SubBrands)
        {
            if (string.IsNullOrWhiteSpace(Name))
                ModelState.AddModelError("Name", "اسم الشركة مطلوب");

            if (ModelState.IsValid)
            {
                var manufacturer = new Manufacturer { Name = Name };

                // رفع الشعار إذا موجود
                if (LogoFile != null && LogoFile.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(LogoFile.FileName);
                    var savePath = Path.Combine(_env.WebRootPath, "logos", fileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);

                    using var stream = new FileStream(savePath, FileMode.Create);
                    await LogoFile.CopyToAsync(stream);

                    manufacturer.LogoPath = "/logos/" + fileName;
                }

                // إضافة SubBrands
                foreach (var sb in SubBrands.Where(s => !string.IsNullOrWhiteSpace(s)))
                {
                    manufacturer.SubBrands.Add(new SubBrand { Name = sb });
                }

                _context.Manufacturers.Add(manufacturer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        // GET: Manufacturers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var manufacturer = await _context.Manufacturers
                .Include(m => m.SubBrands)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (manufacturer == null) return NotFound();

            return View(manufacturer);
        }

        // POST: Manufacturers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string Name, IFormFile? LogoFile, List<string> SubBrands)
        {
            var manufacturer = await _context.Manufacturers
                .Include(m => m.SubBrands)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (manufacturer == null) return NotFound();

            if (ModelState.IsValid)
            {
                manufacturer.Name = Name;

                // لو رفع صورة جديدة
                if (LogoFile != null && LogoFile.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(LogoFile.FileName);
                    var savePath = Path.Combine(_env.WebRootPath, "logos", fileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);

                    using var stream = new FileStream(savePath, FileMode.Create);
                    await LogoFile.CopyToAsync(stream);

                    manufacturer.LogoPath = "/logos/" + fileName;
                }

                // تحديث الـ SubBrands
                manufacturer.SubBrands.Clear();
                foreach (var sb in SubBrands.Where(s => !string.IsNullOrWhiteSpace(s)))
                {
                    manufacturer.SubBrands.Add(new SubBrand { Name = sb });
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(manufacturer);
        }

        // GET: Manufacturers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var manufacturer = await _context.Manufacturers.FirstOrDefaultAsync(m => m.Id == id);
            if (manufacturer == null) return NotFound();

            return View(manufacturer);
        }

        // POST: Manufacturers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var manufacturer = await _context.Manufacturers
                .Include(m => m.SubBrands)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (manufacturer != null)
            {
                _context.SubBrands.RemoveRange(manufacturer.SubBrands);
                _context.Manufacturers.Remove(manufacturer);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
