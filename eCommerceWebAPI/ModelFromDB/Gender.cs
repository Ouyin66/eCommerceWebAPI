using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace eCommerceWebAPI.ModelFromDB
{
    [Table("Gender")]
    [Index(nameof(Name), Name = "UQ__Gender__72E12F1B8E794BA1", IsUnique = true)]
    public partial class Gender
    {
        public Gender()
        {
            Products = new HashSet<Product>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("name")]
        [StringLength(50)]
        public string? Name { get; set; }

        [InverseProperty(nameof(Product.Gender))]
        public virtual ICollection<Product> Products { get; set; }
    }
}
