using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace eCommerceWebAPI.ModelFromDB
{
    [Table("Size")]
    public partial class Size
    {
        public Size()
        {
            Variants = new HashSet<Variant>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("size")]
        [StringLength(50)]
        public string? Name { get; set; }

        [InverseProperty(nameof(Variant.Size))]
        public virtual ICollection<Variant> Variants { get; set; }
    }
}
