using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace P133Allup.Models
{
    public class Brand : BaseEntity
    {
        [StringLength(255)]
        public string Name { get; set; }

        public IEnumerable<Product>? Products { get; set; }

        [NotMapped]
        public IEnumerable<Category> Categories { get; set; }
    }
}
