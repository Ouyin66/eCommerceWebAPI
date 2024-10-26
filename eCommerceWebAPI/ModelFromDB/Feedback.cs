using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace eCommerceWebAPI.ModelFromDB
{
    [Table("Feedback")]
    public partial class Feedback
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("receiptID")]
        public int? ReceiptId { get; set; }
        [Column("variantID")]
        public int? VariantId { get; set; }
        [Column("userName")]
        [StringLength(100)]
        public string? UserName { get; set; }
        [Column("rate", TypeName = "decimal(5, 2)")]
        public decimal? Rate { get; set; }
        [Column("content")]
        [StringLength(255)]
        public string? Content { get; set; }

        [ForeignKey(nameof(ReceiptId))]
        [InverseProperty("Feedbacks")]
        public virtual Receipt? Receipt { get; set; }
        [ForeignKey(nameof(VariantId))]
        [InverseProperty("Feedbacks")]
        public virtual Variant? Variant { get; set; }
    }
}
