using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace eCommerceWebAPI.ModelFromDB
{

    [Table("OrderStatusHistory")]
    public partial class OrderStatusHistory
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("receiptID")]
        public int? ReceiptId { get; set; }
        [Column("state")]
        public int? State { get; set; }
        [Column("notes")]
        public string? Notes { get; set; }
        [Column("timestamp", TypeName = "datetime")]
        public DateTime? TimeStamp { get; set; }

        [ForeignKey(nameof(ReceiptId))]
        [InverseProperty("Receipts")]
        public virtual Receipt? Receipt { get; set; }
    }
}
