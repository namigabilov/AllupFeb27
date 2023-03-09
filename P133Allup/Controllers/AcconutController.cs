using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using P133Allup.Models;
using P133Allup.ViewModels.AcconutViewModel;

namespace P133Allup.Controllers
{
    public class AcconutController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AcconutController(UserManager<AppUser> userManager , RoleManager<IdentityRole> roleManager )
        {
            _roleManager= roleManager;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            return View();
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid) return View(registerVM);

            AppUser appUser = new AppUser
            {
                Name= registerVM.Name,
                Email = registerVM.Email,
                FatherName= registerVM.FatherName,
                UserName= registerVM.UserName,
                SurName= registerVM.SurName,
            };

            IdentityResult identityResult =  await _userManager.CreateAsync(appUser,registerVM.Password);

            if (!identityResult.Succeeded)
            {
                foreach (IdentityError identityError in identityResult.Errors)
                {
                    ModelState.AddModelError("", identityError.Description);
                    return View(registerVM);
                }
            }
            await _userManager.AddToRoleAsync(appUser, "Member");

            return RedirectToAction("index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            return View();
        }


        #region CreateRolesAndSuperAdmin
        //[HttpGet]
        //public async Task<IActionResult> CreateRole()
        //{
        //    await _roleManager.CreateAsync(new IdentityRole("SuperAdmin"));
        //    await _roleManager.CreateAsync(new IdentityRole("Admin"));
        //    await _roleManager.CreateAsync(new IdentityRole("Member"));

        //    return Content("Yaradildi");
        //}
        //[HttpGet]
        //public async Task<IActionResult> CreateUser()
        //{
        //    AppUser appUser = new AppUser
        //    {
        //        Name = "Super",
        //        SurName = "Admin",
        //        FatherName = "SuperAdminFather",
        //        UserName = "SuperAdmin",
        //        Email = "superadmin@gmail.com"
        //    };
        //    var result = await _userManager.CreateAsync(appUser, "Superadmin123");

        //    if (result.Succeeded)
        //    {
        //        await _userManager.AddToRoleAsync(appUser, "SuperAdmin");
        //        return Content("Yaradildi");
        //    }
        //    else
        //    {
        //        return Content("Kullanıcı oluşturma hatası: " + string.Join(", ", result.Errors));
        //    }
        //}
        #endregion
    }
}
