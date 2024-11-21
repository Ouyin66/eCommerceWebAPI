using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace eCommerceWebAPI.ModelFromDB
{
    [Table("Notification")]
    public partial class Notification
    {
        [Key]
        [Column("id")]
        public int? Id { get; set; }

        [Column("userId")]
        public int? UserId { get; set; }

        [Column("message")]
        [StringLength(255)]
        public string? Message { get; set; }

        [Column("dateCreated", TypeName = "datetime")]
        public DateTime DateCreated { get; set; }

        [Column("isRead")]
        public bool? IsRead { get; set; }

        [Column("type")]
        [StringLength(50)]
        public string? Type { get; set; }

        [Column("referenceId")]
        public int? ReferenceId { get; set; }

        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(User.Notifications))]
        public virtual User MyUser { get; set; }
    }
}
