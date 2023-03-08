using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using P133Allup.DataAccessLayer;
using P133Allup.Extentions;
using P133Allup.Helpers;
using P133Allup.Models;
using P133Allup.ViewModels;
using System.Drawing.Drawing2D;

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

        public async Task<IActionResult> Index(int pageindex = 1)
        {
            IQueryable<Category> categories = _context.Categories
                .Include(c => c.Products)
                .Where(c => c.IsDeleted == false && c.IsMain);

            return View(PageNatedList<Category>.Create(categories, pageindex, 5));
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories.Include(c => c.Products.Where(p => p.IsDeleted == false)).Where(c => c.IsDeleted == false).ToListAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
                if ((category.File.Length / 1024) > 3000)
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

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return BadRequest();

            Category category = await _context.Categories.Include(c=>c.Products.Where(p=>p.IsDeleted ==false))
                .Where(c=>c.IsDeleted ==false && c.Id == id && c.IsMain).FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return NotFound();

            return View(category);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteCategory(int? id)
        {
            if(id == null) return BadRequest();

            Category category = await _context.Categories.Include(c => c.Products.Where(c => c.IsDeleted == false))
                .Where(c => c.IsDeleted == false && c.IsMain).FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return NotFound();

            category.IsDeleted = true;
            category.DeletedAt = DateTime.UtcNow.AddHours(4);
            category.DeletedBy = "System";

            foreach (Product product in category.Products)
            {
                product.IsDeleted = true;
                product.DeletedBy = "System";
                product.DeletedAt = DateTime.UtcNow.AddHours(4);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null) return BadRequest();

            Category category = await _context.Categories.Include(c=>c.Products.Where(p=>p.IsDeleted ==false )).Where(c=>c.IsDeleted == false && c.IsMain).FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return NotFound();

            return View(category);
        }

        [HttpGet]
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null) return BadRequest();

            Category category = await _context.Categories.Where(c=>c.IsDeleted == false && c.IsMain).FirstOrDefaultAsync(c=>c.Id == id);

            if (category == null) return NotFound();

            ViewBag.Categories = await _context.Categories.Where(c => c.IsDeleted == false && c.IsMain).ToListAsync();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id , Category category)
        {
            ViewBag.Categories = await _context.Categories.Where(c => c.IsDeleted == false && c.IsMain).ToListAsync();

            if (ModelState.IsValid)
            {
                return View(category);
            }
            if (id == null) return BadRequest();

            if (id != category.Id) return BadRequest();

            Category dbCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted ==false);

            if (dbCategory == null) return NotFound();

            if (category.IsMain)
            {
                if (dbCategory.IsMain)
                {
                    if (category.File != null)
                    {
                        if (category.File.CheckFileContentType("image/jpeg"))
                        {
                            ModelState.AddModelError("File", "Fayl Tipi Duz Deyil");
                            return View(category);
                        }

                        if (category.File.CheckFileLength(300))
                        {
                            ModelState.AddModelError("File", "Fayl Olcusu Maksimum 30 kb Ola Biler");
                            return View(category);
                        }

                        FileHelper.DeleteFile(dbCategory.Image, _env, "assets", "images");

                        dbCategory.Image = await category.File.CraeteFileAsync(_env, "assets", "images");

                    }
                }
                else
                {
                    if (category.File == null)
                    {
                        ModelState.AddModelError("File", "Sekil Mecburidi");
                        return View(category);
                    }

                    if (category.File.CheckFileContentType("image/jpeg"))
                    {
                        ModelState.AddModelError("File", "Fayl Tipi Duz Deyil");
                        return View(category);
                    }

                    if (category.File.CheckFileLength(300))
                    {
                        ModelState.AddModelError("File", "Fayl Olcusu Maksimum 30 kb Ola Biler");
                        return View(category);
                    }

                    dbCategory.Image = await category.File.CraeteFileAsync(_env, "assets", "images");
                }

                dbCategory.ParentId = null;
            }
            else
            {
                if (category.ParentId == null)
                {
                    ModelState.AddModelError("ParentId", "Ust Categorya Mutleq Secilmelidi");
                    return View(category);
                }

                if (!await _context.Categories.AnyAsync(c => c.IsDeleted == false && c.IsMain && c.Id == category.ParentId))
                {
                    ModelState.AddModelError("ParentId", "Duzgun Ust Categorya Sec");
                    return View(category);
                }

                if (dbCategory.Id == category.ParentId)
                {
                    ModelState.AddModelError("ParentId", "Eyni Ola Bilmez");
                    return View(category);
                }
                FileHelper.DeleteFile(dbCategory.Image, _env, "assets", "images");

                dbCategory.Image = null;
                dbCategory.ParentId = category.ParentId;
            }


            dbCategory.Name = category.Name.Trim();
            dbCategory.IsMain = category.IsMain;
            dbCategory.UpdatedBy = "System";
            dbCategory.UpdatedAt = DateTime.UtcNow.AddHours(4);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
