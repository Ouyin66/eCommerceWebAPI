using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace eCommerceWebAPI.ModelFromDB
{
    [Table("Promotion")]
    //[Index(nameof(Code), Name = "UQ__Promotio__357D4CF9E14F211F", IsUnique = true)]
    [Index(nameof(Name), Name = "UQ__Promotio__72E12F1B31B4B4AE", IsUnique = true)]
    public partial class Promotion
    {
        public Promotion()
        {
            Products = new HashSet<Product>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("code")]
        [StringLength(50)]
        public string? Code { get; set; }
        [Column("name")]
        [StringLength(100)]
        public string? Name { get; set; }
        [Column("banner")]
        public byte[]? Banner { get; set; }
        [Column("describe")]
        public string? Describe { get; set; }
        [Column("perDiscount", TypeName = "decimal(5, 2)")]
        public decimal? PerDiscount { get; set; }
        [Column("startDate", TypeName = "datetime")]
        public DateTime? StartDate { get; set; }
        [Column("endDate", TypeName = "datetime")]
        public DateTime? EndDate { get; set; }
        [Column("dateCreate", TypeName = "datetime")]
        public DateTime? DateCreate { get; set; }

        [ForeignKey("PromotionId")]
        [InverseProperty(nameof(Product.Promotions))]
        public virtual ICollection<Product>? Products { get; set; }
    }
}
