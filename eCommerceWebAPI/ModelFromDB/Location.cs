using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace eCommerceWebAPI.ModelFromDB
{
    [Table("Location")]
    public partial class Location
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("userID")]
        public int? UserId { get; set; }
        [Column("name")]
        [StringLength(255)]
        public string? Name { get; set; }
        [Column("address")]
        public string? Address { get; set; }
        [Column("dateCreate", TypeName = "datetime")]
        public DateTime? DateCreate { get; set; }

        [ForeignKey(nameof(UserId))]
        [InverseProperty("Locations")]
        public virtual User? User { get; set; }
    }
}
