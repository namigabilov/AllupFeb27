using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P133Allup.DataAccessLayer;
using P133Allup.Models;
using P133Allup.ViewModels;

namespace P133Allup.Areas.Manage.Controllers
{
    [Area("manage")]

    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;


        public ProductController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index(int pageIndex = 1)
        {
            IQueryable<Product> products =  _context.Products.Where(p => p.IsDeleted == false);

            return View(PageNatedList<Product>.Create(products,pageIndex,5));
        }
        [HttpGet]
        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null) return BadRequest();

            Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.IsDeleted == false);
            
            if (product == null) return NotFound();

            return View(product);
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return BadRequest();

            Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.IsDeleted == false);

            if (product == null) return NotFound();

            return View(product);
        }
        [HttpGet]
        public async Task<IActionResult> DeleteProduct(int? id)
        {
            if(id == null) return BadRequest();

            Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.IsDeleted == false);

            if (product == null) return NotFound();

            product.IsDeleted = true;
            product.DeletedAt = DateTime.UtcNow.AddHours(4);
            product.DeletedBy = "System";

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Brands = await _context.Brands.Where(b => b.IsDeleted == false).ToListAsync();
            ViewBag.Categories = await _context.Categories.Where(b => b.IsDeleted == false && b.IsMain).ToListAsync();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            ViewBag.Brands = await _context.Brands.Where(b => b.IsDeleted == false).ToListAsync();
            ViewBag.Categories = await _context.Categories.Where(b => b.IsDeleted == false && b.IsMain).ToListAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
