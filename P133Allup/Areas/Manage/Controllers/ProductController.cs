using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P133Allup.DataAccessLayer;
using P133Allup.Extentions;
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
            ViewBag.Tags = await _context.Tags.Where(t => t.IsDeleted == false).ToListAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            ViewBag.Brands = await _context.Brands.Where(b => b.IsDeleted == false).ToListAsync();
            ViewBag.Categories = await _context.Categories.Where(b => b.IsDeleted == false && b.IsMain).ToListAsync();
            ViewBag.Tags = await _context.Tags.Where(t => t.IsDeleted == false).ToListAsync();

            if (!ModelState.IsValid)
            {
                return View(product);
            }

            if (!await _context.Brands.AnyAsync(b=>b.IsDeleted ==false &&b.Id ==product.BrandId))
            {
                ModelState.AddModelError("BrandId", "Daxil Olunan Brand Yanlisdir !");
                return View(product);
            }
            if (!await _context.Categories.AnyAsync(b => b.IsDeleted == false && b.Id == product.CategoryId))
            {
                ModelState.AddModelError("CategoryId", "Daxil Olunan Category Yanlisdir !");
                return View(product);
            }

            if (product.TagIds != null && product.TagIds.Count() > 0)
            {
                List<ProductTag> productTags = new List<ProductTag>();

                foreach (byte tags in product.TagIds)
                {
                    if (!await _context.Categories.AnyAsync(b => b.IsDeleted == false && b.Id == tags))
                    {
                        ModelState.AddModelError("TagIds", $"Daxil Olunan Tag ${tags} Yanlisdir !");
                        return View(product);
                    }

                    ProductTag productTag = new ProductTag
                    {
                        TagId = tags,
                        CreatedAt = DateTime.UtcNow.AddHours(4),
                        CreatedBy = "System"
                    };

                    productTags.Add(productTag);
                }

                product.ProductTags = productTags;
            }
            else
            {
                ModelState.AddModelError("TagIds", $"Yanlisdir !");
                return View(product);
            }

            if (product.MainFile != null)
            {
                if (!product.MainFile.CheckFileContentType("image/jpeg"))
                {
                    ModelState.AddModelError("ManiFile", $" Yanlisdir !");
                    return View(product);
                }
                if (!product.MainFile.CheckFileLength(300))
                {
                    ModelState.AddModelError("ManiFile", $" Yanlisdir !");
                    return View(product);
                }

                product.MainImage = await product.MainFile.CraeteFileAsync(_env, "assets", "images", "product");
            }
            else
            {
                ModelState.AddModelError("ManiFile", $" Yanlisdir !");
                return View(product);
            }

            if (product.HoverFile != null)
            {
                if (!product.HoverFile.CheckFileContentType("image/jpeg"))
                {
                    ModelState.AddModelError("HoverFile", $" Yanlisdir !");
                    return View(product);
                }
                if (!product.HoverFile.CheckFileLength(300))
                {
                    ModelState.AddModelError("HoverFile", $" Yanlisdir !");
                    return View(product);
                }

                product.HoverImage = await product.HoverFile.CraeteFileAsync(_env, "assets", "images", "product");
            }
            else
            {
                ModelState.AddModelError("HoverFile", $" Yanlisdir !");
                return View(product);
            }

            if (product.Files != null && product.Files.Count() > 0)
            {
                List<ProductImage> productImages = new List<ProductImage>();

                foreach ( IFormFile file in product.Files ) 
                {
                    if (!file.CheckFileContentType("image/jpeg"))
                    {
                        ModelState.AddModelError("Files", $" Yanlisdir !");
                        return View(product);
                    }
                    if (!file.CheckFileLength(300))
                    {
                        ModelState.AddModelError("Files", $" Yanlisdir !");
                        return View(product);
                    }

                    ProductImage productImage = new ProductImage
                    {
                        Image = await file.CraeteFileAsync(_env,"assets","images","product"),
                        CreatedAt= DateTime.UtcNow.AddHours(4),
                        CreatedBy = "Sytsem"
                    };
                }

                product.ProductImages= productImages;
            }

            string seria = "";
            seria = seria + _context.Brands.FirstOrDefault(c => c.Id == product.BrandId).Name.Substring(0, 2);
            seria = seria + _context.Categories.FirstOrDefault(c => c.Id == product.CategoryId).Name.Substring(0, 2);

            product.Seria = seria;
            product.Code = _context.Products.Where(p => p.Seria == product.Seria).OrderByDescending(p => p.Id).FirstOrDefault() != null ?
                _context.Products.Where(p => p.Seria == product.Seria).OrderByDescending(p => p.Id).FirstOrDefault().Code + 1 : 1;

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            Product product = await _context.Products
                .Include(p=>p.ProductImages.Where(pi=>pi.IsDeleted ==false ))
                .FirstOrDefaultAsync(p=>p.Id == id && p.IsDeleted ==false);

            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Brands = await _context.Brands.Where(b => b.IsDeleted == false).ToListAsync();
            ViewBag.Categories = await _context.Categories.Include(c=>c.Children.Where(chil=>chil.IsDeleted == false))
                .Where(b => b.IsDeleted == false && b.IsMain).ToListAsync();
            ViewBag.Tags = await _context.Tags.Where(t => t.IsDeleted == false).ToListAsync();

            product.TagIds = (product.ProductTags != null && product.ProductTags.Count() > 0) ?
                product.ProductTags.Select(x => (byte)x.Id).ToList() : new List<byte>();

            return View(product);
        }
    }
}
