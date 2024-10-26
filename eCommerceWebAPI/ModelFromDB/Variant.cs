using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace eCommerceWebAPI.ModelFromDB
{
    // Serialize your object

    [Table("Variant")]
    public partial class Variant
    {
        public Variant()
        {
            Carts = new HashSet<Cart>();
            Feedbacks = new HashSet<Feedback>();
            ReceiptVariants = new HashSet<ReceiptVariant>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("productID")]
        public int? ProductId { get; set; }
        [Column("picture")]
        [MaxLength]
        public string? Picture { get; set; }
        [Column("colorID")]
        public int? ColorId { get; set; }
        [Column("sizeID")]
        public int? SizeId { get; set; }
        [Column("price", TypeName = "decimal(18, 2)")]
        public decimal? Price { get; set; }
        [Column("quantity")]
        public int? Quantity { get; set; }
        [Column("dateCreate", TypeName = "datetime")]
        public DateTime? DateCreate { get; set; }

        [ForeignKey(nameof(ColorId))]
        [InverseProperty("Variants")]
        public virtual Color? Color { get; set; }
        [ForeignKey(nameof(ProductId))]
        [InverseProperty("Variants")]
        public virtual Product? Product { get; set; }
        [ForeignKey(nameof(SizeId))]
        [InverseProperty("Variants")]
        public virtual Size? Size { get; set; }
        [InverseProperty(nameof(Cart.Variant))]
        public virtual ICollection<Cart> Carts { get; set; }
        [InverseProperty(nameof(Feedback.Variant))]
        public virtual ICollection<Feedback> Feedbacks { get; set; }
        [InverseProperty(nameof(ReceiptVariant.Variant))]
        public virtual ICollection<ReceiptVariant> ReceiptVariants { get; set; }
    }
}
