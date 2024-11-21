using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace eCommerceWebAPI.ModelFromDB
{
    [Table("Picture")]
    public partial class Picture
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("productID")]
        public int? ProductId { get; set; }
        [Column("image")]
        public string? Image { get; set; }

        [ForeignKey(nameof(ProductId))]
        [InverseProperty("Pictures")]
        public virtual Product? Product { get; set; }
    }
}
