using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace eCommerceWebAPI.ModelFromDB
{
    [Table("Messenger")]
    public partial class Messenger
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("userID")]
        public int? UserId { get; set; }
        [Column("chatID")]
        public int? ChatId { get; set; }
        [Column("message")]
        [StringLength(255)]
        public string? Message { get; set; }
        [Column("state")]
        public byte? State { get; set; }
        [Column("dateCreate", TypeName = "datetime")]
        public DateTime? DateCreate { get; set; }

        [ForeignKey(nameof(ChatId))]
        [InverseProperty("Messengers")]
        public virtual Chat? Chat { get; set; }
        [ForeignKey(nameof(UserId))]
        [InverseProperty("Messengers")]
        public virtual User? User { get; set; }
    }
}
