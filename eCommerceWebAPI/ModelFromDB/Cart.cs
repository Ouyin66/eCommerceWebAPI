using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace eCommerceWebAPI.ModelFromDB
{
    [Table("Cart")]
    public partial class Cart
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("userID")]
        public int? UserId { get; set; }
        [Column("variantID")]
        public int? VariantId { get; set; }
        [Column("quantity")]
        public int? Quantity { get; set; }
        [Column("price", TypeName = "decimal(18, 2)")]
        public decimal? Price { get; set; }

        [ForeignKey(nameof(UserId))]
        [InverseProperty("Carts")]
        public virtual User? User { get; set; }
        [ForeignKey(nameof(VariantId))]
        [InverseProperty("Carts")]
        public virtual Variant? Variant { get; set; }
    }
}
