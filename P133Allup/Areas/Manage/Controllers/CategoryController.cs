using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using P133Allup.DataAccessLayer;
using P133Allup.Models;

namespace P133Allup.Areas.Manage.Controllers
{
    [Area("manage")]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public CategoryController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }


        public async Task<IActionResult> Index()
        {
            return View(await _context.Categories.Include(c=>c.Products.Where(p=>p.IsDeleted==false)).Where(c=>c.IsMain && c.IsDeleted ==false).ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories.Include(c => c.Products.Where(p => p.IsDeleted == false)).Where(c => c.IsMain && c.IsDeleted == false).ToListAsync();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Category category)
        {
            ViewBag.Categories = await _context.Categories.Include(c => c.Products.Where(p => p.IsDeleted == false)).Where(c => c.IsMain && c.IsDeleted == false).ToListAsync();

            if (ModelState.IsValid)
            {
                return View(category);
            }
            if (category.Name == null)
            {
                ModelState.AddModelError("Name", "Name Bos Qosula Bilmez");
                return View(category);
            }
            if (await _context.Categories.AnyAsync(c=> c.IsDeleted == false && c.Name.ToLower().Contains(category.Name.Trim().ToLower())))
            {
                ModelState.AddModelError("Name", "Bu Adda Category Artiq Movcudur !!");
                return View(category);
            }
            if (category.IsMain)
            {
                if (category.File == null)
                {
                    ModelState.AddModelError("File", "File Mutleq Daxil Edilmelidir !!");
                    return View(category);
                }

                if (category.File.ContentType != "image/jpeg")
                {
                    ModelState.AddModelError("File", "File Novu Duzgun Deyil !!");
                    return View();
                }
                if ((category.File.Length / 1024) > 300)
                {
                    ModelState.AddModelError("File", "File Olcusu Max 300 Kb ola biler !");
                    return View();
                }

                string fileName = $"{DateTime.UtcNow.ToString("yyyyMMddHHmmssfff")}-{Guid.NewGuid().ToString()}-{category.File.FileName}";

                string filePath = Path.Combine(_env.WebRootPath, "assets", "images", fileName);

                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    await category.File.CopyToAsync(stream);
                }

                category.Image = fileName;

                category.ParentId = null;
            }
            else
            {
                if (category.ParentId == null)
                {
                    ModelState.AddModelError("File", "Ust Category Mutleq Secilmelidir !!");
                    return View(category);
                }

                if (!await _context.Categories.AnyAsync(c=>c.IsDeleted == false && c.IsMain && c.Id == category.ParentId))
                {
                    ModelState.AddModelError("File", "Duzgun Ust Category Sec");
                    return View(category);  
                }

                category.Image = null;
            }

            category.Name = category.Name.Trim();
            category.CreatedAt= DateTime.UtcNow.AddHours(4);
            category.CreatedBy = "System";

            await _context.AddAsync(category);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

    }
}
