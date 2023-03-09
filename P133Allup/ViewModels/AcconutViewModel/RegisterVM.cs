using System.ComponentModel.DataAnnotations;

namespace P133Allup.ViewModels.AcconutViewModel
{
    public class RegisterVM
    {
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(100)]
        public string? SurName { get; set; }

        [StringLength(100)]
        public string? FatherName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string UserName { get; set;}

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }


    }
}
