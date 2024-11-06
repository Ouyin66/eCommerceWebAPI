using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace eCommerceWebAPI.ModelFromDB
{
    [Table("Receipt")]
    public partial class Receipt
    {
        public Receipt()
        {
            Feedbacks = new HashSet<Feedback>();
            ReceiptVariants = new HashSet<ReceiptVariant>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("userID")]
        public int? UserId { get; set; }
        [Column("address")]
        [StringLength(255)]
        public string? Address { get; set; }
        [Column("phone")]
        [StringLength(10)]
        public string? Phone { get; set; }
        [Column("coupon")]
        public int? Coupon { get; set; }
        [Column("total", TypeName = "decimal(18, 2)")]
        public decimal? Total { get; set; }
        [Column("paymentMethod")]
        public byte? PaymentMethod { get; set; }
        [Column("state")]
        public byte? State { get; set; }
        [Column("dateCreate", TypeName = "datetime")]
        public DateTime? DateCreate { get; set; }

        [ForeignKey(nameof(UserId))]
        [JsonIgnore]
        [InverseProperty("Receipts")]
        public virtual User? User { get; set; }

        [InverseProperty(nameof(Feedback.Receipt))]
        [JsonIgnore]
        public virtual ICollection<Feedback> Feedbacks { get; set; }

        [InverseProperty(nameof(ReceiptVariant.Receipt))]
        public virtual ICollection<ReceiptVariant> ReceiptVariants { get; set; }
    }
}
