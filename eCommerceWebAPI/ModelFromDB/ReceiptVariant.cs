using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace eCommerceWebAPI.ModelFromDB
{
    [Table("Receipt_Variant")]
    public partial class ReceiptVariant
    {
        [Key]
        [Column("variantID")]
        public int VariantId { get; set; }
        [Key]
        [Column("receiptID")]
        public int ReceiptId { get; set; }
        [Column("quantity")]
        public int? Quantity { get; set; }
        [Column("price", TypeName = "decimal(18, 2)")]
        public decimal? Price { get; set; }

        [ForeignKey(nameof(ReceiptId))]
        [JsonIgnore]
        [InverseProperty("ReceiptVariants")]
        public virtual Receipt Receipt { get; set; } = null!;

        [ForeignKey(nameof(VariantId))]
        [JsonIgnore]
        [InverseProperty("ReceiptVariants")]
        public virtual Variant Variant { get; set; } = null!;
    }
}
