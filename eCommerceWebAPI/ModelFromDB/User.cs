using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace eCommerceWebAPI.ModelFromDB
{
    [Table("User")]
    [Index(nameof(Email), Name = "UQ__User__AB6E6164A8D86AA7", IsUnique = true)]
    public partial class User
    {
        public User()
        {
            Carts = new HashSet<Cart>();
            Chats = new HashSet<Chat>();
            Messengers = new HashSet<Messenger>();
            Receipts = new HashSet<Receipt>();
            Products = new HashSet<Product>();
            Locations = new HashSet<Location>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("email")]
        [StringLength(100)]
        public string? Email { get; set; }
        [Column("password")]
        [StringLength(255)]
        public string? Password { get; set; }
        [Column("name")]
        [StringLength(100)]
        public string? Name { get; set; }
        [Column("phone")]
        [StringLength(10)]
        public string? Phone { get; set; }
        [Column("defaultLocationId")]
        public int? DefaultLocationId { get; set; }
        [Column("image")]
        public string? Image { get; set; }
        [Column("gender")]
        public byte? Gender { get; set; }
        [Column("role")]
        public byte? Role { get; set; }
        [Column("providerID")]
        [StringLength(255)]
        public string? ProviderId { get; set; }
        [Column("state")]
        public byte? State { get; set; }
        [Column("dateCreate", TypeName = "datetime")]
        public DateTime? DateCreate { get; set; }

        [InverseProperty(nameof(Cart.User))]
        public virtual ICollection<Cart> Carts { get; set; }
        [InverseProperty(nameof(Chat.User))]
        public virtual ICollection<Chat> Chats { get; set; }
        [InverseProperty(nameof(Messenger.User))]
        public virtual ICollection<Messenger> Messengers { get; set; }
        [InverseProperty(nameof(Receipt.User))]
        public virtual ICollection<Receipt> Receipts { get; set; }
        [InverseProperty(nameof(Location.User))]
        public virtual ICollection<Location> Locations { get; set; }

        public virtual Location? DefaultLocation { get; set; }

        [ForeignKey("UserId")]
        [InverseProperty(nameof(Product.Users))]
        public virtual ICollection<Product> Products { get; set; }
    }
}
