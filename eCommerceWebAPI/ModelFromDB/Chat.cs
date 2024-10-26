using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace eCommerceWebAPI.ModelFromDB
{
    [Table("Chat")]
    public partial class Chat
    {
        public Chat()
        {
            Messengers = new HashSet<Messenger>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("userID")]
        public int? UserId { get; set; }
        [Column("dateCreate", TypeName = "datetime")]
        public DateTime? DateCreate { get; set; }

        [ForeignKey(nameof(UserId))]
        [InverseProperty("Chats")]
        public virtual User? User { get; set; }
        [InverseProperty(nameof(Messenger.Chat))]
        public virtual ICollection<Messenger> Messengers { get; set; }
    }
}
