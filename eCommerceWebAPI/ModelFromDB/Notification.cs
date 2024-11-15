using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace eCommerceWebAPI.ModelFromDB
{
    [Table("Notification")]
    public partial class Notification
    {
        [Key]
        [Column("Id")]
        public int? Id { get; set; }

        [Column("UserId")]
        public int? UserId { get; set; }

        [Column("Message")]
        [StringLength(255)]
        public string? Message { get; set; }

        [Column("DateCreated", TypeName = "datetime")]
        public DateTime DateCreated { get; set; }

        [Column("IsRead")]
        public bool? IsRead { get; set; }

        [Column("Type")]
        [StringLength(50)]
        public string? Type { get; set; }

        [Column("ReferenceId")]
        public int? ReferenceId { get; set; }

        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(User.Notifications))]
        public virtual User MyUser { get; set; }
    }
}
